using AttnSoft.AutoUpdate;
using AttnSoft.Upgrad;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AttnSoft.Upgrad;
internal class Program
{
    static async Task Main(string[] args)
    {
        UpdateLog.Category = "Upgrade";
        UpdateLog.MinimumLevel = LogLevel.Debug;

        // 本进程是被主程序启动的独立进程，CreateNoWindow 且可能提权，调试器和 DebugView 都够不着，
        // 只能靠文件留痕。逐行落盘是因为安装完成会以 Kill() 结束自身，缓冲的日志会跟着丢。
        // 安装目录不可写时退到临时目录：日志写不出不能反过来中断安装，而写不出恰是最需要留痕的时刻。
        var installLogError = EnableFileLog(AppContext.BaseDirectory);
        if (installLogError != null && EnableFileLog(Path.GetTempPath()) == null)
            UpdateLog.Warning("无法在安装目录创建升级日志，已改写到临时目录", installLogError);

        // 宿主启动本进程后立刻 Environment.Exit(0)，本进程是唯一能把主程序拉回来的进程。
        // 所以安装失败也要重启，只是不带升级成功标记、不清备份。
        // new UpgradApp() 自身失败时 upgradApp 为 null，救不了：StartAppCmd 只存在于 newVersionInfo.json 里，此时不知道该启动谁。
        UpgradApp? upgradApp = null;
        try
        {
            UpdateLog.Information("The AttnSoft.Upgrade is starting to install...");
            upgradApp = new UpgradApp();
            await upgradApp.StartInstall();
            UpdateLog.Information("Installation is complete.");
            // 结束自身原本写在 StartApp(true) 里、排在这行日志之前 ⇒ 成功日志永远落不了盘，
            // 「装成功了」和「装到一半崩了」在日志里就只剩 Critical 在不在这一条区别。
            upgradApp.KillSelf();
        }
        catch (Exception ex)
        {
            UpdateLog.Critical("Install Error", ex);
            if (upgradApp != null)
            {
                UpdateLog.Warning("安装失败，仍尝试启动主程序（不带升级成功标记）");
                try { upgradApp.StartApp(false); }
                catch (Exception startEx) { UpdateLog.Error("失败路径启动主程序也失败", startEx); }
            }
        }
    }

    /// <summary>启用文件日志；失败时保持默认 Trace sink 不变并返回异常，由调用方决定是否换目录重试。</summary>
    static Exception? EnableFileLog(string root)
    {
        try
        {
            UpdateLog.Sink = new FileUpdateLogger(Path.Combine(root, "upgrade_logs", "upgrade.log"));
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

}
