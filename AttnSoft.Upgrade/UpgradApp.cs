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
        var sourcePath = Context.AppPath;
        var targetPath = Context.PatchPath;
        var backupDirectory = Context.BackupPath;

        try
        {
            await DifferentialCore.Instance?.Dirty(sourcePath, targetPath, backupDirectory);
        }
        catch (Exception e)
        {
            Console.WriteLine("出错了:" + e.ToString());
        }

        Clear(patchPath);
        var newfilename = "newVersionInfo.json";
        newfilename = Path.Combine(sourcePath, newfilename);
        var localfilename = "Version.json";
        localfilename = Path.Combine(sourcePath, localfilename);
        File.Move(newfilename, localfilename, true);
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
