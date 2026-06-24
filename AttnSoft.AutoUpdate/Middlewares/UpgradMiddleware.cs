using AttnSoft.AutoUpdate.Common;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate.Middlewares;
public interface IUpgrad : IApplicationMiddleware<UpdateContext> { }
/// <summary>
/// Upgrade yourself.升级自身的Upgrade
/// </summary>
public class UpgradMiddleware : IUpgrad
{
    public async Task InvokeAsync(ApplicationDelegate<UpdateContext> next, UpdateContext context)
    {
        try
        {
            Console.WriteLine("[Upgrad] Start UpgradMiddleware");
            var sourcePath = context.AppPath;
            var patchPath = context.PatchPath;
            var upgradeName = context.UpgradName;

#if NETFRAMEWORK
            string resUpgradeName = "AttnSoft.AutoUpdate.Upgrade.Framework.Upgrade.exe";
            string exeExtension = ".exe";
#else
            string resUpgradeName = "AttnSoft.AutoUpdate.Upgrade.AOT.Upgrade.exe";
            string exeExtension = OperatingSystem.IsWindows() ? ".exe" : "";
#endif
            System.Reflection.Assembly app = System.Reflection.Assembly.GetExecutingAssembly();
            Stream? fsOpen = app.GetManifestResourceStream(resUpgradeName);
            if (fsOpen != null)
            {
                context.UpgradName = "Upgrade" + exeExtension;
                var filename = Path.Combine(sourcePath, context.UpgradName);

                // 杀掉可能残留的 Upgrade 进程
                ProcessHelper.KillByNameInDir(sourcePath, upgradeName);

                // 删除旧文件
                if (File.Exists(filename))
                {
                    File.SetAttributes(filename, FileAttributes.Normal);
                    File.Delete(filename);
                }

                // 直接写入目标文件
                using (fsOpen)
                using (var fsSave = new FileStream(filename, FileMode.Create))
                {
                    Console.WriteLine("[Upgrad] Writing Upgrade...");
                    await fsOpen.CopyToAsync(fsSave);
                }
                Console.WriteLine($"[Upgrad] Extracted Upgrade to {filename}");
            }
            else
            {
                Console.WriteLine($"[Upgrad] Embedded resource not found ({resUpgradeName}), copying from patchPath: {patchPath}");
                upgradeName = Path.GetFileNameWithoutExtension(upgradeName);
                Console.WriteLine($"[Upgrad] Looking for upgrade files with base name: {upgradeName}");
                string[] upgradeFiles = new string[] {
                upgradeName,
                $"{upgradeName}.exe",
                $"{upgradeName}.dll",
                $"{upgradeName}.deps.json",
                $"{upgradeName}.runtimeconfig.json"
                };
                foreach (var upgradeFile in upgradeFiles)
                {
                    var newUpgradeFile = Path.Combine(patchPath, upgradeFile);
                    if (File.Exists(newUpgradeFile))
                    {
                        var oldUpgradeFile = Path.Combine(sourcePath, upgradeFile);
                        if (File.Exists(oldUpgradeFile))
                        {
                            File.SetAttributes(oldUpgradeFile, FileAttributes.Normal);
                            File.Delete(oldUpgradeFile);
                        }
                        File.Move(newUpgradeFile, oldUpgradeFile);
                        Console.WriteLine($"[Upgrad] Copied {upgradeFile} from patch to sourcePath");
                    }
                }
            }
            Console.WriteLine("[Upgrad] Calling next middleware (StartUpgrad)");
            await next(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Upgrad] Exception: {ex}");
            throw;
        }
    }
}
