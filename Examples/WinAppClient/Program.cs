
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
        static void Main(string[] args)
        {
            SetProcessDPIAware();
            //StartUpdate();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

    }
}