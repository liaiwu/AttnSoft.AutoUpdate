using AttnSoft.AutoUpdate;

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
                FmUpdate fmUpdate = new FmUpdate();
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
       
                // 事件今后在线程池线程触发。ShowUpdateInfo 被调用时窗体还没 Show 过、句柄未创建，
                // 此时 InvokeRequired 返回 false，守卫拦不住（实测照旧跑在线程池线程上），
                // 所以用 UI 调度器把整个调用归位；窗体因此在 UI 线程上 Show，
                // 后续 ChangProgress / DownloadComplet 里的 InvokeRequired 守卫才成立。
                var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                context.OnFindNewVersion = (next, context) =>
                    Task.Factory.StartNew(() => fmUpdate.ShowUpdateInfo(next, context),
                        CancellationToken.None, TaskCreationOptions.None, uiScheduler);
                context.OnUpdateException = (ex) =>
                {
                    Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                };
                context.OnDownloadCompleted += () => 
                { 
                    Console.WriteLine("The upgrade package has been downloaded successfully.");
                    fmUpdate.DownloadComplet();
                };

                context.OnDownloadProgressChanged = (long arg1, int value) => 
                { 
                    Console.WriteLine($"Current download : Total size:{arg1}, Progress percentage:{value}%");
                    fmUpdate.ChangProgress(value);
                };

                await UpdateApp.CreateBuilder(context).StartUpdateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
    }
}
