using System.Threading.Tasks;
using WinAppClient;

namespace ConsoleUpdate
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"当前版本:{version}");

            await myUpdateApp.StartUpdate();
            Console.ReadKey();
        }
    }
}
