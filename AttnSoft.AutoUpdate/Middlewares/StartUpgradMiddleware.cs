using AttnSoft.AutoUpdate.Common;
using AttnSoft.AutoUpdate.JsonContext;
using GeneralUpdate.Common.FileBasic;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
            var appPath = Path.Combine(context.AppPath, context.UpgradName);

            if (File.Exists(ProcessInfoFileName))
            {
                File.SetAttributes(ProcessInfoFileName, FileAttributes.Normal);
                File.Delete(ProcessInfoFileName);
            }
            if (string.IsNullOrEmpty(context.UpdateVersion.StartAppCmd))
            {
                context.UpdateVersion.StartAppCmd = context.StartAppCmd;
            }
#if NETFRAMEWORK
            var ProcessInfo = DefaultJsonConvert.JsonConvert.Serialize(context.UpdateVersion);
#else
            var ProcessInfo = DefaultJsonConvert.JsonConvert.Serialize(context.UpdateVersion, VersionInfoJsonContext.Default.VersionInfo);
#endif
            File.WriteAllText(ProcessInfoFileName, ProcessInfo);

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

            Process.Start(processStartInfo);
            Process.GetCurrentProcess().Kill();

            string path = context.TempPath;
            if (Directory.Exists(path))
                StorageManager.DeleteDirectory(path);
            //await next(context);
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
        else
        {
            ExeUpdateAsync(context);
        }
        await next(context);
    }

}