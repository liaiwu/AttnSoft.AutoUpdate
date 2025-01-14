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
                Console.WriteLine($"�������ʼ����{DateTime.Now}��");
                Console.WriteLine("��ǰ����Ŀ¼��" + Thread.GetDomain().BaseDirectory);
                await Task.Delay(100);

                var context = new UpdateContext()
                {
                    ClientVersion = new Version("1.0.0.0"),
                    //UpdateUrl = "http://127.0.0.1:5000/Verification"
                    UpdateUrl = "http://update.attnsoft.com/demo/v1/version.json"
                };
                //ʹ����WebApi��ʽ��ȡ�������汾��Ϣ
                //context.UseWebApiGetVersionInfo();
                //�����ָ��ʹ��WebApi��ʽ,�ڲ�Ĭ��ʹ��OSS��ʽ
  
                context.OnUpdateException = (ex) =>
                {
                    Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                };
                context.OnDownloadCompleted = ()=> { Console.WriteLine("�������������"); };

                context.OnDownloadProgressChanged =(long arg1, int arg2)=> { Console.WriteLine($"��ǰ���ذ汾���ܴ�С��{arg1}, ���Ȱٷֱȣ�{arg2}%"); };
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