using AttnSoft.Upgrad;
using System;
using System.Threading.Tasks;

namespace AttnSoft.Upgrad;
internal class Program
{
    static async Task Main(string[] args)
    {

        Console.WriteLine($"{DateTime.Now}：升级程序AttnSoft.Upgrade开始安装……");
        //Console.WriteLine("请按任意键继续……");
        //Console.ReadKey();
        try
        {
            UpgradApp upgradApp = new UpgradApp();
            await upgradApp.StartInstall();
            Console.WriteLine($"安装结束，{DateTime.Now}！");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Install Error: " + ex.Message + Environment.NewLine + ex.StackTrace);
            Console.ReadKey();
        }
    }

}
