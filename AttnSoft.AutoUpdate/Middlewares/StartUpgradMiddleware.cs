using AttnSoft.AutoUpdate.Common;
using GeneralUpdate.Common.FileBasic;
using GeneralUpdate.Common.JsonContext;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
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
    public async Task InvokeAsync(ApplicationDelegate<UpdateContext> next, UpdateContext context)
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
            var ProcessInfo = JsonSerializer.Serialize(context.UpdateVersion, VersionInfoJsonContext.Default.VersionInfo);
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
                UseShellExecute = true
            };
            processStartInfo.Arguments = Utils.BuildArguments(arguments);
            Process.Start(processStartInfo);
            string path = context.TempPath;
            if (Directory.Exists(path))
                StorageManager.DeleteDirectory(path);
            await next(context);
        }
        catch (Exception ex)
        {
            context.OnUpdateException?.Invoke(ex);
        }
        finally
        {
            Process.GetCurrentProcess().Kill();
        }
    }

}