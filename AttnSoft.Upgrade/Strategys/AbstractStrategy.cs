using GeneralUpdate.Common.FileBasic;
using System;
using System.IO;
using System.Threading.Tasks;
using UpgradExpress.Objects;

namespace Upgrad.Strategys
{
    public abstract class AbstractStrategy : IStrategy
    {
        protected const string Patchs = "patchs";
        
        public virtual Task Execute() => throw new NotImplementedException();
        
        public virtual void StartApp() => throw new NotImplementedException();
        
        public virtual Task ExecuteAsync() => throw new NotImplementedException();

        public virtual void Create(UpgradContext content) => throw new NotImplementedException();

        //protected static void OpenBrowser(string url)
        //{
        //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //    {
        //        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        //        return;
        //    }
            
        //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        //    {
        //        Process.Start("xdg-open", url);
        //        return;
        //    }
            
        //    throw new PlatformNotSupportedException("Unsupported OS platform");
        //}
        
        protected static void Clear(string path)
        {
            if (Directory.Exists(path))
                StorageManager.DeleteDirectory(path);
        }
    }
}