using AttnSoft.AutoUpdate.Common;
using AttnSoft.AutoUpdate.JsonContext;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate.Middlewares;
/// <summary>
/// 启动Upgrade进行安装
/// </summary>
public interface IStartUpgrad : IApplicationMiddleware<UpdateContext> { }
/// <summary>
/// 启动Upgrade进行安装中间件
/// </summary>
public class StartUpgradMiddleware : IStartUpgrad
{
    /// <summary>
    /// 重启应用程序安装新版本
    /// </summary>
    /// <param name="context"></param>
    public static void ExeUpdateAsync(UpdateContext context)
    {
        UpdateLog.Debug("[StartUpgrad] === Start ExeUpdateAsync ===");
        try
        {
            // 确保应用目录下没有已运行的升级器进程
            ProcessHelper.KillByNameInDir(context.AppPath, context.UpgradName);

            const string ProcessInfoFileName = "newVersionInfo.json";
            var processInfoFilePath = Path.Combine(context.AppPath, ProcessInfoFileName);
            var appPath = Path.Combine(context.AppPath, context.UpgradName);
            UpdateLog.Debug($"[StartUpgrad] Upgrade path: {appPath}");

            if (File.Exists(processInfoFilePath))
            {
                File.SetAttributes(processInfoFilePath, FileAttributes.Normal);
                File.Delete(processInfoFilePath);
            }
            if (string.IsNullOrEmpty(context.UpdateVersion.StartAppCmd))
            {
                context.UpdateVersion.StartAppCmd = context.StartAppCmd;
            }

            // 安全检查：防止服务端控制的 StartAppCmd 路径遍历攻击
            if (!string.IsNullOrEmpty(context.UpdateVersion.StartAppCmd))
            {
                var cmd = context.UpdateVersion.StartAppCmd;
                if (cmd.IndexOf('\\') >= 0 || cmd.IndexOf('/') >= 0 || cmd.IndexOf("..") >= 0)
                {
                    throw new SecurityException(
                        $"Invalid StartAppCmd: path traversal detected '{cmd}'");
                }
            }
#if NETFRAMEWORK
            var ProcessInfo = DefaultJsonConvert.JsonConvert.Serialize(context.UpdateVersion);
#else
            var ProcessInfo = DefaultJsonConvert.JsonConvert.Serialize(context.UpdateVersion, VersionInfoJsonContext.Default.VersionInfo);
#endif
            File.WriteAllText(processInfoFilePath, ProcessInfo);
            UpdateLog.Debug($"[StartUpgrad] Written {processInfoFilePath}");

            var arguments = new Collection<string>
                {
                    "--appPath",
                    context.AppPath,
                    "--backupPath",
                    context.BackupPath,
                    "--patchPath",
                    context.PatchPath,
                };
            var processStartInfo = new ProcessStartInfo
            {
                FileName = appPath,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            processStartInfo.Arguments = Utils.BuildArguments(arguments);
            UpdateLog.Debug($"[StartUpgrad] Arguments: {processStartInfo.Arguments}");

            try
            {
                UpdateLog.Debug("[StartUpgrad] About to call Process.Start(...)");
                var process = Process.Start(processStartInfo);
                if (process == null)
                    throw new InvalidOperationException($"Failed to start upgrade process: {appPath}");
            }
            catch (Exception ex)
            {
                UpdateLog.Error("[StartUpgrad] 启动 Upgrade 进程失败", ex);
                context.OnUpdateException?.Invoke(ex);
                return;
            }

            UpdateLog.Information("[StartUpgrad] 升级器已启动，即将结束当前进程");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            UpdateLog.Error("[StartUpgrad] 准备启动升级器时失败", ex);
            context.OnUpdateException?.Invoke(ex);
        }
    }

    public async Task InvokeAsync(ApplicationDelegate<UpdateContext> next, UpdateContext context)
    {
        try
        {
            if (context.OnBeforeInstall != null)
            {
                UpdateLog.Debug("[StartUpgrad] Calling OnBeforeInstall callback");
                context.OnBeforeInstall.Invoke(context);
            }
        }
        catch (Exception ex)
        {
            UpdateLog.Error("[StartUpgrad] 消费者的 OnBeforeInstall 回调抛出异常，升级中止", ex);
            context.OnUpdateException?.Invoke(ex);
            return;
        }
        ExeUpdateAsync(context);
    }

}