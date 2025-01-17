using AttnSoft.AutoUpdate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAppClient
{
    internal class myUpdateApp
    {
        public static UpdateContext UpContext=new UpdateContext();

        internal static async Task StartUpdate()
        {
            try
            {
                Console.WriteLine($"{DateTime.Now}:Initiate the upgrade program.");
                Console.WriteLine("Current working directory:" + Thread.GetDomain().BaseDirectory);

                //UpdateContext? context = new UpdateContext()
                //{
                //    ClientVersion = new Version("1.0.0.0"),
                //    //UpdateUrl = "http://127.0.0.1:5000/Verification"
                //    UpdateUrl = "http://update.attnsoft.com/demo/v1/version.json"
                //};
                var context = UpContext;
                context.UpdateUrl = "http://update.attnsoft.com/demo/v1/version.json";
                //context.UpdateUrl = "http://127.0.0.1:5000/Verification";
                //使用用WebApi方式获取服务器版本信息
                //context.UseWebApi();
                //如果不指定使用WebApi方式,内部默认使用OSS方式

                context.OnUpdateException = (ex) =>
                {
                    Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                };
                context.OnDownloadCompleted = () => { Console.WriteLine("The upgrade package has been downloaded successfully.");};

                context.OnDownloadProgressChanged = (long arg1, int arg2) => { Console.WriteLine($"Current download version: Total size:{arg1}, Progress percentage:{arg2}%"); };

                await UpdateApp.CreateBuilder(context).StartUpdateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
    }
}
