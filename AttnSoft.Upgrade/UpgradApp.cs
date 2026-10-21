using AttnSoft.AutoUpdate;
using AttnSoft.AutoUpdate.Common;
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
        // 扫描并关闭应用目录下的所有进程
        ProcessHelper.KillAllInDir(Context.AppPath);


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

        FileBusyRetry.Run($"删除 {localfilename}", () => File.Delete(localfilename));
        FileBusyRetry.Run($"写入 {localfilename}", () => File.Move(newfilename, localfilename));
        
        StartApp(true);
    }
    protected static void Clear(string path)
    {
        if (Directory.Exists(path))
            StorageManager.DeleteDirectory(path);
    }

    /// <param name="succeeded">false 表示安装失败后的补救重启：不带升级成功标记、不清备份。</param>
    public void StartApp(bool succeeded)
    {
        var mainFileName = Path.Combine(Context.AppPath, Context.UpdateVersion.StartAppCmd);
        if (string.IsNullOrEmpty(mainFileName) || !File.Exists(mainFileName))
        {
            UpdateLog.Error($"Startup program not found: {mainFileName}");
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = mainFileName,
                UseShellExecute = true,
                // 这个标记会让宿主的 CheckCompletionMiddleware 触发 OnUpdateCompleted，安装失败时不能带
                Arguments = succeeded ? "AttnSoft.AutoUpdate.Successful" : string.Empty
            });
        }
        catch(Exception ex)
        {
            UpdateLog.Error($"启动主程序失败: {mainFileName}", ex);
            return;
        }

        if (!succeeded) return;   // 失败时备份要留给回滚
        // 备份没清掉只是收尾瑕疵，不能反过来否定一次已经成功的升级：StorageManager.DeleteDirectory
        // 不吞异常，这里不兜住就会冒到 Program.Main 的 catch，把刚起来的主程序再起一遍。
        try { Clear(Context.BackupPath); }
        catch (Exception ex) { UpdateLog.Warning($"清理备份目录失败，不影响本次升级结果: {Context.BackupPath}", ex); }
    }

    /// <summary>安装成功后结束自身。放在 Main 里而不是 StartApp 里，是为了让成功日志能先写出来。</summary>
    public void KillSelf() => Process.GetCurrentProcess().Kill();
}
