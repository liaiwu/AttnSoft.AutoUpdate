using AttnSoft.AutoUpdate;
using AttnSoft.AutoUpdate.Middlewares;
using GeneralUpdate.Common.HashAlgorithms;
using Microsoft.Extensions.DependencyInjection;

namespace WinAppClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //var md5Hash = new Sha256HashAlgorithm();
            //this.richTextBox1.Text = md5Hash.ComputeHash("WinAppClient.zip");
            //var verOss = new VersionOSS();
            //verOss.PubTime = DateTime.Now;
            //this.richTextBox1.Text = JsonSerializer.Serialize(verOss);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //var md5Hash = new Sha256HashAlgorithm();
            //this.richTextBox1.Text = md5Hash.ComputeHash("packet_20241125233523804_1.0.0.1.zip");
        }
        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //测量运行时间
            //var stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();

            //await StartUpdate();

            //stopwatch.Stop();
            //Console.WriteLine($"StartUpdate函数运行时间：{stopwatch.ElapsedMilliseconds} 毫秒");
        }
        private async Task StartUpdate()
        {
            try
            {
                Console.WriteLine($"主程序初始化，{DateTime.Now}！");
                Console.WriteLine("当前运行目录：" + Thread.GetDomain().BaseDirectory);
                await Task.Delay(100);

                var context = new UpdateContext()
                { 
                    ClientVersion = new Version("1.0.0.0"),
                    UpdateUrl = "http://127.0.0.1:5000/Verification" 
                    //UpdateUrl = "http://localhost/packages/version.json"
                };
                context.UseWebApiGetVersionInfo();
                //context.Services.AddSingleton<ICheckUpdate, CheckUpdateUseWebApiMiddleware>();
                context.OnUpdateException += OnUpdateException;
                context.OnDownloadCompleted += OnDownloadCompleted;
                context.OnDownloadProgressChanged += OnDownloadProgressChanged;
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

                await UpdateApp.CreateBuilder(context).StartUpdateAsync() ;
                //await builder.StartUpdateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void OnDownloadProgressChanged(long arg1, int arg2)
        {
            Console.WriteLine($"当前下载版本：总大小：{arg1}, 进度百分比：{arg2}%");
        }

        private void OnDownloadCompleted()
        {
            Console.WriteLine("升级包下载完成");
        }

        void OnUpdateException(Exception ex)
        {
            Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
        }


    }
}
