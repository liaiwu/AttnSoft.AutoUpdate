
using AttnSoft.AutoUpdate;
using System.Runtime.InteropServices;


namespace WinAppClient
{
    internal static class Program
    {

        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main(string[] args)
        {

            //启动后自动启动升级
            //UpdateContext? context = new UpdateContext()
            //{
            //    UpdateUrl = "http://update.attnsoft.com/demo/v1/version.json"
            //};
            //context.UseWebApi();

            //context.OnUpdateException = (ex) =>
            //{
            //    Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            //};
            //context.OnDownloadCompleted = () => { Console.WriteLine("The upgrade package has been downloaded successfully."); };
            //context.OnDownloadProgressChanged = (long arg1, int arg2) => { Console.WriteLine($"Current download version: Total size:{arg1}, Progress percentage:{arg2}%"); };
            //await UpdateApp.CreateBuilder(context).StartUpdateAsync();

#if NETCOREAPP
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
#else
            SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#endif
            Application.Run(new Form1());
        }

    }
}