using AttnSoft.Upgrad;
using System;
using System.Threading.Tasks;

namespace AttnSoft.Upgrad;
internal class Program
{
    static async Task Main(string[] args)
    {

        Console.WriteLine($"{DateTime.Now}:The AttnSoft.Upgrade is starting to install...");
#if DEBUG
        Console.WriteLine("Please press any key to continue...");
        Console.ReadKey();
#endif
        try
        {
            UpgradApp upgradApp = new UpgradApp();
            await upgradApp.StartInstall();
            Console.WriteLine($"{DateTime.Now}:Installation is complete.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Install Error: " + ex.Message + Environment.NewLine + ex.StackTrace);
            Console.ReadKey();
        }
    }

}
