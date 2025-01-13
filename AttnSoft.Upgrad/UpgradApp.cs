using GeneralUpdate.Common.FileBasic;
using GeneralUpdate.Differential;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AttnSoft.Upgrad;

internal class UpgradApp
{
    UpgradContext Context = new UpgradContext();
    public async Task StartInstall()
    {

        var patchPath = Context.PatchPath;

        var version = Context.UpdateVersion;
        try
        {
            var sourcePath = Context.AppPath;
            var targetPath = Context.PatchPath;
            var backupDirectory = Context.BackupPath;
            await DifferentialCore.Instance?.Dirty(sourcePath, targetPath, backupDirectory);

        }
        catch (Exception e)
        {
            //status = ReportType.Failure;
            Console.WriteLine("出错了:" + e.ToString());
            //EventManager.Instance.Dispatch(this, new ExceptionEventArgs(e, e.Message));
        }
        //if (!string.IsNullOrEmpty(_configinfo.UpdateLogUrl))
        //{
        //    OpenBrowser(_configinfo.UpdateLogUrl);
        //}
        Clear(patchPath);
        StartApp();


    }
    protected static void Clear(string path)
    {
        if (Directory.Exists(path))
            StorageManager.DeleteDirectory(path);
    }

    public void StartApp()
    {
        try
        {
            var mainAppPath = Path.Combine(Context.AppPath, Context.UpdateVersion.StartAppCmd);
            Process.Start(mainAppPath);
        }
        finally
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}
