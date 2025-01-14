using AttnSoft.AutoUpdate;

namespace WinAppClient
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            StartUpdate();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
        private static async Task StartUpdate()
        {
            try
            {
                Console.WriteLine($"主程序初始化，{DateTime.Now}！");
                Console.WriteLine("当前运行目录：" + Thread.GetDomain().BaseDirectory);
                await Task.Delay(100);

                var context = new UpdateContext()
                {
                    ClientVersion = new Version("1.0.0.0"),
                    //UpdateUrl = "http://127.0.0.1:5000/Verification"
                    UpdateUrl = "http://update.attnsoft.com/demo/v1/version.json"
                };
                //使用用WebApi方式获取服务器版本信息
                //context.UseWebApiGetVersionInfo();
                //如果不指定使用WebApi方式,内部默认使用OSS方式
  
                context.OnUpdateException = (ex) =>
                {
                    Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                };
                context.OnDownloadCompleted = ()=> { Console.WriteLine("升级包下载完成"); };

                context.OnDownloadProgressChanged =(long arg1, int arg2)=> { Console.WriteLine($"当前下载版本：总大小：{arg1}, 进度百分比：{arg2}%"); };
                //context.OnGetUpdateVersionInfo = (context) =>
                //{
                //    return new List<VersionInfo>
                //    {
                //        new VersionInfo
                //        {
                //            Version = new Version("1.0.0.1"),
                //        }
                //    };
                //};
                await UpdateApp.CreateBuilder(context).StartUpdateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
    }
}