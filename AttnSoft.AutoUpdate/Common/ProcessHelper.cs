using System;
using System.Diagnostics;
using System.IO;

namespace AttnSoft.AutoUpdate.Common;

/// <summary>
/// 进程操作工具类
/// </summary>
public static class ProcessHelper
{
    /// <summary>
    /// 杀掉应用目录下指定名称的进程
    /// </summary>
    /// <param name="appPath">应用目录</param>
    /// <param name="processName">进程名（含或不含扩展名均可，如 "Upgrade" 或 "Upgrade.exe"）</param>
    public static void KillByNameInDir(string appPath, string processName)
    {
        var name = Path.GetFileNameWithoutExtension(processName);
        foreach (var p in Process.GetProcesses())
        {
            if (p.ProcessName != name)
                continue;

            try
            {
                var mainModule = p.MainModule?.FileName;
                if (mainModule != null
                    && mainModule.StartsWith(appPath, StringComparison.OrdinalIgnoreCase))
                {
                    if (!p.HasExited)
                    {
                        p.Kill();
                        p.WaitForExit(5000);
                        Console.WriteLine($"[ProcessHelper] Killed process: {name} (pid={p.Id})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProcessHelper] Failed to kill process {name} (pid={p.Id}): {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 杀掉应用目录下的所有进程（排除自身）
    /// </summary>
    /// <param name="appPath">应用目录</param>
    public static void KillAllInDir(string appPath)
    {
        var selfPid = Process.GetCurrentProcess().Id;
        foreach (var p in Process.GetProcesses())
        {
            if (p.Id == selfPid)
                continue;

            try
            {
                var mainModule = p.MainModule?.FileName;
                if (mainModule != null
                    && mainModule.StartsWith(appPath, StringComparison.OrdinalIgnoreCase))
                {
                    if (!p.HasExited)
                    {
                        p.Kill();
                        p.WaitForExit(5000);
                        Console.WriteLine($"[ProcessHelper] Killed process PID={p.Id} ({p.ProcessName})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProcessHelper] Process check skipped (pid={p.Id}): {ex.Message}");
            }
        }
    }
}
