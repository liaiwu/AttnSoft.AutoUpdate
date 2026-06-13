using AttnSoft.AutoUpdate.Common;
using AttnSoft.AutoUpdate.JsonContext;
using GeneralUpdate.Common.FileBasic;
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
        try
        {
            const string ProcessInfoFileName = "newVersionInfo.json";
            var processInfoFilePath = Path.Combine(context.AppPath, ProcessInfoFileName);
            var appPath = Path.Combine(context.AppPath, context.UpgradName);

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

            var arguments = new Collection<string>
                {
                    "--appPath",
                    context.AppPath,
                    "--backupPath",
                    context.BackupPath,
                    "--patchPath",
                    context.PatchPath
                };
            var processStartInfo = new ProcessStartInfo
            {
                FileName = appPath,
                UseShellExecute = true,
#if !DEBUG
                WindowStyle = ProcessWindowStyle.Hidden, // 隐藏启动窗口
                CreateNoWindow = true // 不创建新窗口
#endif
            };
            processStartInfo.Arguments = Utils.BuildArguments(arguments);

            try
            {
                var process = Process.Start(processStartInfo);
                if (process == null)
                    throw new InvalidOperationException($"Failed to start upgrade process: {appPath}");
            }
            catch (Exception ex)
            {
                context.OnUpdateException?.Invoke(ex);
                return;
            }

            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            context.OnUpdateException?.Invoke(ex);
        }
    }

    public async Task InvokeAsync(ApplicationDelegate<UpdateContext> next, UpdateContext context)
    {
        if (context.OnBeforeInstall != null)
        {
            context.OnBeforeInstall.Invoke(context);
        }
        ExeUpdateAsync(context);
        //await next(context);
    }

}