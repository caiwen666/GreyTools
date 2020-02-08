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
        public static string info;
        public static string tools;
        public c_info ci=new c_info();
        public C_Tools ct = new C_Tools();
        Dictionary<string, Object> dllList = new Dictionary<string, Object>();
        Dictionary<string, Type> _dllList = new Dictionary<string, Type>();
        public Form1() : base("http://tools.greycloud.com/www/new.html")
        {
            //加载info
            //Debug.WriteLine(info);
            ci = JsonConvert.DeserializeObject<c_info>(info);
            ct= JsonConvert.DeserializeObject<C_Tools>(tools);
            InitializeComponent();
            LoadHandler.OnLoadStart += LoadHandler_OnLoadStart;
            DragHandler.OnDragEnter += DragHandler_OnDragEnter;
            LoadHandler.OnLoadEnd += LoadHandler_OnLoadEnd;
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
            var ToolsManager = GlobalObject.AddObject("ToolsManager");
            var runTools = ToolsManager.AddFunction("runTools");
            runTools.Execute += (func, args) =>
            {
                var stringArgument = args.Arguments.FirstOrDefault(p => p.IsString);
                if (stringArgument != null)
                {
                    this.runTools(stringArgument.StringValue);
                }
            };
        }

        private void DragHandler_OnDragEnter(object sender, Chromium.Event.CfxOnDragEnterEventArgs e)
        {
            e.SetReturnValue(true);//屏蔽外部拖拽进来的链接
        }
        private void Form1_Load(object sender, EventArgs e)
        {

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
            dllList.Add(name, o);
            _dllList.Add(name, t);
            //配置
            t.GetField("registerFunc").SetValue(o, registerFunc);
        }
        public void registerFunc()
        {

        }
    }
    public class c_info
    {
        public bool debug;
    }
    public class Tools
    {
        public string name { get; set; }
        public string packName { get; set; }
        public string version { get; set; }
        public string author { get; set; }
        public string introduction { get; set; }
        public string runWithGT { get; set; }
        public string alwaysRun { get; set; }

    }

    public class C_Tools
    {
        public List<Tools> tools { get; set; }
        public int count { get; set; }
        public int RWGTcount { get; set; }
    }
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
                i++;
                int p = i / t_ct.RWGTcount * 100;
                t_ExecuteJavascript("$('#progress').text('" + i + "/" + t_ct.RWGTcount + "');$('#state').text('加载" + a.name + "[" + a.packName + "]');$('#progress_bar').css('width','" + p + "%');");
                Thread.Sleep(5000);
            }
            t_ExecuteJavascript("fl.close();");
        }
    }
}