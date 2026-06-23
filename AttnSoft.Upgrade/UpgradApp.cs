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
        if (Context.MainPid.HasValue)
        {
            try
            {
                var mainProcess = Process.GetProcessById(Context.MainPid.Value);
                mainProcess.WaitForExit(5000);
            }
            catch (ArgumentException)
            {
                // 主进程已经退出，无需等待
            }
        }

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

        File.Delete(localfilename);
        File.Move(newfilename, localfilename);
        
        StartApp();
    }
    protected static void Clear(string path)
    {
        if (Directory.Exists(path))
            StorageManager.DeleteDirectory(path);
    }

    public void StartApp()
    {
        var mainFileName = Path.Combine(Context.AppPath, Context.UpdateVersion.StartAppCmd);
        if (string.IsNullOrEmpty(mainFileName) || !File.Exists(mainFileName))
        {
            Console.WriteLine($"Startup program not found: {mainFileName}");
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = mainFileName,
                UseShellExecute = true,
                Arguments = "AttnSoft.AutoUpdate.Successful"
            });
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Failed to start application: {mainFileName}");
            Console.WriteLine($"Error: {ex.Message}");
            return;
        }

        Clear(Context.BackupPath);
        Process.GetCurrentProcess().Kill();
    }
}
