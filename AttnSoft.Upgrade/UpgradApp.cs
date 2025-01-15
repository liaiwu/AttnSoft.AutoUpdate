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
        if (string.IsNullOrEmpty(sourcePath))
        {
            throw new  ArgumentNullException(nameof(sourcePath));
        }
        if (string.IsNullOrEmpty(targetPath))
        {
            throw new ArgumentNullException(nameof(targetPath));
        }
        if (string.IsNullOrEmpty(backupDirectory))
        {
            throw new ArgumentNullException(nameof(backupDirectory));
        }
        await DifferentialCore.Instance?.Dirty(sourcePath, targetPath, backupDirectory);

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
            var mainFileName = Path.Combine(Context.AppPath, Context.UpdateVersion.StartAppCmd);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = mainFileName,
                UseShellExecute = true,
                Arguments= "AttnSoft.AutoUpdate.Successful"
            };
            Process.Start(processStartInfo);
        }
        finally
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}
