using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetDimension.NanUI;
using System.IO;

namespace GreyTools
{
    static class Program
    {
        public static void throwError(String info)
        {
            MessageBox.Show(info, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(-1);
        }
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try {
                // 创建一个 StreamReader 的实例来读取文件 
                // using 语句也能关闭 StreamReader
                using (StreamReader sr = new StreamReader("info.json"))
                {
                    string line;
                    // 从文件读取并显示行，直到文件的末尾 
                    while ((line = sr.ReadLine()) != null)
                    {
                        Form1.info = Form1.info + line + "\n";
                    }
                }
            }
            catch (Exception e)
            {
                // 向用户显示出错消息
                throwError("在加载时出现错误:找不到info.json\n" + e.Message);
            }
            try
            {
                // 创建一个 StreamReader 的实例来读取文件 
                // using 语句也能关闭 StreamReader
                using (StreamReader sr = new StreamReader("tools/tools.json"))
                {
                    string line;
                    // 从文件读取并显示行，直到文件的末尾 
                    while ((line = sr.ReadLine()) != null)
                    {
                        Form1.tools = Form1.tools + line + "\n";
                    }
                }
            }
            catch (Exception e)
            {
                // 向用户显示出错消息
                throwError("在加载时出现错误:找不到tools.json\n" + e.Message);
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //UIStartupManager.UseSharedFramework = true;
            //指定CEF架构和文件目录结构，并初始化CEF
            if (Bootstrap.Load(settings =>
            {
                //禁用日志
                settings.LogSeverity = Chromium.CfxLogSeverity.Disable;
                //指定中文为当前CEF环境的默认语言
                settings.AcceptLanguageList = "zh-CN";
                settings.Locale = "zh-CN";
            }, commandLine =>
            {
                //在启动参数中添加disable-web-security开关，禁用跨域安全检测
                commandLine.AppendSwitch("disable-web-security");

            }))
            {
                //注册嵌入资源，并为指定资源指定一个假的域名
                Bootstrap.RegisterAssemblyResources(Resources.Main.GetSchemeAssembley(), null, "tools.greycloud.com");
                Application.Run(new Form1());
            }
        }
    }
}
