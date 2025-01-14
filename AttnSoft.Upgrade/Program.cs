using AttnSoft.Upgrad;
using System;
using System.Threading.Tasks;

namespace AttnSoft.Upgrad;
internal class Program
{
    static async Task Main(string[] args)
    {

        Console.WriteLine($"升级程序初始化，{DateTime.Now}！");
        Console.WriteLine("请按任意键继续……");
        Console.ReadKey();
        try
        {
            UpgradApp upgradApp = new UpgradApp();
            await upgradApp.StartInstall();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
        }
        Console.WriteLine($"升级程序已启动，{DateTime.Now}！");

    }

}
