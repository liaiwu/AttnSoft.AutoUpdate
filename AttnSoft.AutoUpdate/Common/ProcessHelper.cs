using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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
                var path = TryGetProcessPath(p, out int lastErr);
                if (path == null)
                {
                    // 进程名已命中却读不到路径。87 = 进程在枚举和取路径之间已经退出（实测 pid=0 走的就是 87），
                    // 无害；5 = 没权限，那才是「该杀的没杀掉」。MinimumLevel 默认是 Information，
                    // 后者不记 Warning 在客户机上一行痕迹都不留。
                    if (lastErr == ERROR_INVALID_PARAMETER)
                        UpdateLog.Debug($"[ProcessHelper] 进程已退出，跳过: {name} (pid={p.Id})");
                    else
                        UpdateLog.Warning($"[ProcessHelper] 命中进程名但读不到路径 (lastErr={lastErr})，未结束它: {name} (pid={p.Id})");
                }
                else if (IsUnderDir(path, appPath))
                {
                    if (!p.HasExited)
                    {
                        p.Kill();
                        if (p.WaitForExit(5000))
                            UpdateLog.Information($"[ProcessHelper] Killed process: {name} (pid={p.Id})");
                        else
                            UpdateLog.Warning($"[ProcessHelper] {name} (pid={p.Id}) 未在 5 秒内退出，它占用的文件仍会导致安装失败");
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateLog.Warning($"[ProcessHelper] Failed to kill process {name} (pid={p.Id})", ex);
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
                var path = TryGetProcessPath(p, out _);
                if (path != null && IsUnderDir(path, appPath))
                {
                    if (!p.HasExited)
                    {
                        p.Kill();
                        if (p.WaitForExit(5000))
                            UpdateLog.Information($"[ProcessHelper] Killed process PID={p.Id} ({p.ProcessName})");
                        else
                            UpdateLog.Warning($"[ProcessHelper] PID={p.Id} ({p.ProcessName}) 未在 5 秒内退出，它占用的文件仍会导致安装失败");
                    }
                }
            }
            catch (Exception ex)
            {
                // 全表遍历：读不到其他进程的路径属正常情况（权限不足），不是失败
                UpdateLog.Debug($"[ProcessHelper] Process check skipped (pid={p.Id})", ex);
            }
        }
    }

    /// <summary>
    /// 取进程的可执行文件路径，取不到返回 null（权限不足即返回 null，不抛）。
    /// Windows 上不用 Process.MainModule：32 位进程读 64 位进程时它必抛
    /// Win32Exception(299 ERROR_PARTIAL_COPY)，实测 0/187 全失败，且 Modules 走同一条通道一起废。
    /// QueryFullProcessImageNameW 跨位数可用（实测 32→64、64→32、64→64 均成功），所以不必判断位数。
    /// </summary>
    /// <param name="lastErr">取不到时的 Win32 错误码（成功时为 0），供调用方区分「没权限」和「进程已不在」。</param>
    static string? TryGetProcessPath(Process p, out int lastErr)
    {
        lastErr = 0;
        if (IsWindowsHost())
        {
            IntPtr h = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, p.Id);
            if (h == IntPtr.Zero)
            {
                lastErr = Marshal.GetLastWin32Error();
                UpdateLog.Debug($"[ProcessHelper] OpenProcess 失败 (pid={p.Id}) lastErr={lastErr}");
                return null;
            }
            try
            {
                var sb = new StringBuilder(1024);
                int capacity = sb.Capacity;
                if (QueryFullProcessImageName(h, 0, sb, ref capacity)) return sb.ToString();
                lastErr = Marshal.GetLastWin32Error();
                UpdateLog.Debug($"[ProcessHelper] QueryFullProcessImageName 失败 (pid={p.Id}) lastErr={lastErr}");
                return null;
            }
            finally { CloseHandle(h); }
        }

        // Unix 上 MainModule 不抛而是返回空集合（dotnet/runtime ProcessManager.Linux.cs:76、
        // ProcessManager.BSD.cs:53 把错误全吞掉），只挂 catch 会完全静默，所以 null 也要记一行。
        string? path = null;
        Exception? error = null;
        try { path = p.MainModule?.FileName; }
        catch (Exception ex) { error = ex; }
        if (path == null) UpdateLog.Debug($"[ProcessHelper] 读不到进程路径 (pid={p.Id})", error);
        return path;
    }

    const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
    const int ERROR_INVALID_PARAMETER = 87;

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr hObject);

    // flags=0 即 PROCESS_NAME_WIN32，返回 C:\... 形态；传 1 会返回 \Device\HarddiskVolumeN\ 形态，不能用于路径比较
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, ref int lpdwSize);

    /// <summary>
    /// mainModule 是否位于 appPath 目录下。要求目录名完整匹配：appPath 为 C:\Program Files\Foo 时
    /// 匹配 C:\Program Files\Foo\tool.exe，不匹配 C:\Program Files\FooBar\tool.exe。
    /// </summary>
    static bool IsUnderDir(string mainModule, string appPath)
    {
        bool isWindows = IsWindowsHost();
        if (isWindows) appPath = appPath.Replace('/', '\\');
        var dir = appPath.TrimEnd(Path.DirectorySeparatorChar);
        return mainModule.Length > dir.Length
            && mainModule[dir.Length] == Path.DirectorySeparatorChar
            && mainModule.StartsWith(dir, isWindows ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    // OperatingSystem.IsWindows() 是 .NET Framework 4.7.1 才有的 API，本类要编 net461，只能按框架分开
    static bool IsWindowsHost()
    {
#if NETFRAMEWORK
        return true;
#else
        return OperatingSystem.IsWindows();
#endif
    }
}
