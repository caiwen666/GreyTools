using Chromium.Remote;
using Chromium.WebBrowser;
using NetDimension.NanUI;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GreyTools
{
    public delegate bool d_ExecuteJavascript(string t);
    public partial class Form1 : Formium
    {
        public static string info;//启动信息
        public static string tools;//工具信息
        public c_info ci=new c_info();//启动信息
        public C_Tools ct = new C_Tools();//工具信息
        Dictionary<string,ITools > dllList = new Dictionary<string, ITools>();//存放已经加载的工具信息
        JSObject ToolsManager;
        public Form1() : base("http://tools.greycloud.com/www/new.html")
        {
            //加载配置信息
            ci = JsonConvert.DeserializeObject<c_info>(info);
            ct= JsonConvert.DeserializeObject<C_Tools>(tools);

            InitializeComponent();

            //绑定事件
            LoadHandler.OnLoadStart += LoadHandler_OnLoadStart;
            DragHandler.OnDragEnter += DragHandler_OnDragEnter;
            LoadHandler.OnLoadEnd += LoadHandler_OnLoadEnd;

            //greyTools_api
            var greyTools_api = GlobalObject.AddObject("greyTools_api");
            var setColor = greyTools_api.AddFunction("setColor");
            setColor.Execute += (func, args) =>
            {
                var stringArgument = args.Arguments.FirstOrDefault(p => p.IsString);
                if (stringArgument != null)
                {
                    this.BorderColor = System.Drawing.ColorTranslator.FromHtml(stringArgument.StringValue);
                }
            };

            //windowsApi
            var windows_api = GlobalObject.AddObject("windows_api");
            var openWeb = windows_api.AddFunction("openWeb");
            openWeb.Execute += (func, args) =>
            {
                var stringArgument = args.Arguments.FirstOrDefault(p => p.IsString);
                if (stringArgument != null)
                {
                    System.Diagnostics.Process.Start(stringArgument.StringValue);
                }
            };

            //Tools管理器
            ToolsManager = GlobalObject.AddObject("ToolsManager");
            var runTools = ToolsManager.AddFunction("runTools");
            runTools.Execute += (func, args) =>
            {
                var stringArgument = args.Arguments.FirstOrDefault(p => p.IsString);
                if (stringArgument != null)
                {
                    bool result=this.runTools(stringArgument.StringValue);
                    args.SetReturnValue(CfrV8Value.CreateBool(result));
                }
            };
        }

        private void DragHandler_OnDragEnter(object sender, Chromium.Event.CfxOnDragEnterEventArgs e)
        {
            e.SetReturnValue(true);//屏蔽外部拖拽进来的链接
        }
        private void LoadHandler_OnLoadEnd(object sender, Chromium.Event.CfxOnLoadEndEventArgs e)
        {
            // Check if it is the main frame when page has loaded.
            if (e.Frame.IsMain)
            {
                //开线程
                RunTools t = new RunTools();
                t.t_ExecuteJavascript = ExecuteJavascript;
                t.t_ct = ct;
                Thread thread = new Thread(new ThreadStart(t.RunWith));
                thread.Start();
                
            }
        }
        private void LoadHandler_OnLoadStart(object sender, Chromium.Event.CfxOnLoadStartEventArgs e)
        {
            if (ci.debug)
            {
                Chromium.ShowDevTools();
            }
        }

        public bool runTools(string name)
        {
            if (dllList.ContainsKey(name))
            {
                return false;
            }
            //挂载dll
            Assembly asm = Assembly.LoadFrom("tools\\"+name+"\\"+"ui.dll");
            //读取类型
            Type t = asm.GetType(name + ".Main");
            //创建对象信息
            object o = Activator.CreateInstance(t);
            Bootstrap.RegisterAssemblyResources(asm, null, name+".greycloud.com");
            ITools it = new ITools(name,asm,t,ToolsManager.AddObject(name),o);
            dllList.Add(name, it);
            //配置
            Type del1 = t.GetNestedType("d_registerFunc");
            if (del1 != null)
            {
                try {
                    Delegate d = Delegate.CreateDelegate(del1, this.GetType().GetMethod("registerFunc"));
                    t.GetField("registerFunc").SetValue(o, d);
                }
                catch(ArgumentException e)
                {
                    Debug.WriteLine(e.ParamName);
                }
                
            }
            //执行Load
            MethodInfo m_Load = t.GetMethod("Load", new Type[] { typeof(int) });
            bool result = (bool)m_Load.Invoke(o, new object[] { 0 });
            return result;
        }
        /// <summary>
        /// 在ToolsManager中新建一个参数
        /// </summary>
        /// <param name="packName">包名</param>
        /// <param name="name">注册函数名字</param>
        public void registerFunc(string packName,string name)
        {
            //ToolsManager.
            ITools it = dllList[packName];
            var fun = it.js.AddFunction(name);
            fun.Execute += (func, args) =>
            {
                var stringArgument = args.Arguments.FirstOrDefault(p => p.IsString);
                if (stringArgument != null)
                {
                    MethodInfo m_func = it.type.GetMethod("func_"+name, new Type[] { typeof(string) });
                    string result = (string)m_func.Invoke(it.obj, new object[] { stringArgument.StringValue });
                    args.SetReturnValue(CfrV8Value.CreateString(result));
                }
            };
        }
    }

    /// <summary>
    /// 工具在dlllist中的信息类
    /// </summary>
    public class ITools
    {
        public string name;
        public Assembly asm;
        public Type type;
        public JSObject js;
        public Object obj;
        public ITools(string name,Assembly asm,Type type,JSObject js,Object obj)
        {
            this.name = name;
            this.asm = asm;
            this.type = type;
            this.js = js;
            this.obj = obj;
        }
    }

    /// <summary>
    /// 启动配置json信息类
    /// </summary>
    public class c_info
    {
        public bool debug;
    }
    /// <summary>
    /// 工具的json信息类
    /// </summary>
    public class Tools
    {
        public string name { get; set; }
        public string packName { get; set; }
        public string version { get; set; }
        public string author { get; set; }
        public string introduction { get; set; }
        public bool runWithGT { get; set; }
        public bool alwaysRun { get; set; }

    }

    /// <summary>
    /// 工具总表的json信息类
    /// </summary>
    public class C_Tools
    {
        public List<Tools> tools { get; set; }
        public int count { get; set; }
        public int RWGTcount { get; set; }
    }

    /// <summary>
    /// 自启动时加载工具的线程类
    /// </summary>
    class RunTools
    {
        public C_Tools t_ct;
        public d_ExecuteJavascript t_ExecuteJavascript;
        public void RunWith()
        {
            //开始加载了
            t_ExecuteJavascript("fl=null;fl=new mdui.Dialog('#firstLoad',{modal:true,closeOnEsc:false,destroyOnClosed:true});fl.open();$('#progress').text('0/" + t_ct.RWGTcount + "');");
            int i = 0;
            foreach (Tools a in t_ct.tools)
            {
                if (!a.alwaysRun)
                {
                    continue;
                }
                i++;
                int p = i / t_ct.RWGTcount * 100;
                t_ExecuteJavascript("$('#progress').text('" + i + "/" + t_ct.RWGTcount + "');$('#state').text('加载" + a.name + "[" + a.packName + "]');$('#progress_bar').css('width','" + p + "%');");
                Thread.Sleep(5000);
            }
            t_ExecuteJavascript("fl.close();");
        }
    }
}