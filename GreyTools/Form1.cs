using NetDimension.NanUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GreyTools
{
    public partial class Form1 : Formium
    {
        public Form1() : base("http://tools.greycloud.com/www/new.html")
        {
            InitializeComponent();
            LoadHandler.OnLoadStart += LoadHandler_OnLoadStart;
            var greyTools_api = GlobalObject.AddObject("greyTools_api");
            var setColor = greyTools_api.AddFunction("setColor");
            setColor.Execute += (func, args) =>
            { 
                var stringArgument = args.Arguments.FirstOrDefault(p => p.IsString);
                if (stringArgument != null)
                {
                    this.BorderColor=System.Drawing.ColorTranslator.FromHtml(stringArgument.StringValue);
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
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void LoadHandler_OnLoadStart(object sender, Chromium.Event.CfxOnLoadStartEventArgs e)
        {
            Chromium.ShowDevTools();
        }
    }
}
