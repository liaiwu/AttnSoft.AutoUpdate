using AttnSoft.AutoUpdate;
using System;
using System.IO;
using System.Threading;

namespace GeneralUpdate.Common.FileBasic;

/// <summary>
/// 安装期文件操作的重试。进程刚被结束、杀软或索引服务还攥着句柄时，文件操作会抛
/// sharing/lock violation，这类等一下就好；其余 IOException（磁盘满、路径不存在、
/// 权限不足）重试不会变好，直接抛出去。
/// </summary>
internal static class FileBusyRetry
{
    const int Attempts = 5;
    const int DelayMs = 1000;

    // 别在这里加「整次安装的总预算」：文件被锁死时 Run 一抛就中断调用方的 foreach，等待累加不起来；
    // 而「每个文件都被短暂锁住」的场景里重试正在起作用，砍预算等于打断一次本来会成功的安装。
    // 两个场景都由 _probe_upg 的 run_budget.sh / retry 探针实测过。

    public static void Run(string what, Action op)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                op();
                return;
            }
            catch (IOException ex) when (IsBusy(ex))
            {
                if (attempt >= Attempts)
                {
                    UpdateLog.Warning($"{what}：重试 {Attempts} 次仍被占用，放弃", ex);
                    throw;
                }
                // 升级器是 CreateNoWindow 启动的，没有任何界面，不记就是静默卡住
                UpdateLog.Debug($"{what}：第 {attempt}/{Attempts} 次被占用，{DelayMs}ms 后重试");
                Thread.Sleep(DelayMs);
            }
        }
    }

    // ERROR_SHARING_VIOLATION(32) / ERROR_LOCK_VIOLATION(33)
    static bool IsBusy(IOException ex)
    {
        int code = ex.HResult & 0xFFFF;
        return code == 32 || code == 33;
    }
}
