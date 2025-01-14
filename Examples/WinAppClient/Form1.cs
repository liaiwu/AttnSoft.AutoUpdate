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
            //��������ʱ��
            //var stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();

            //await StartUpdate();

            //stopwatch.Stop();
            //Console.WriteLine($"StartUpdate��������ʱ�䣺{stopwatch.ElapsedMilliseconds} ����");
        }
        private async Task StartUpdate()
        {
            try
            {
                Console.WriteLine($"�������ʼ����{DateTime.Now}��");
                Console.WriteLine("��ǰ����Ŀ¼��" + Thread.GetDomain().BaseDirectory);
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
            Console.WriteLine($"��ǰ���ذ汾���ܴ�С��{arg1}, ���Ȱٷֱȣ�{arg2}%");
        }

        private void OnDownloadCompleted()
        {
            Console.WriteLine("�������������");
        }

        void OnUpdateException(Exception ex)
        {
            Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
        }


    }
}
