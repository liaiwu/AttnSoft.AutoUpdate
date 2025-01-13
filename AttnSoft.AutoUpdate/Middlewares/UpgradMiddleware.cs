using System.IO;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate.Middlewares;
public interface IUpgrad : IApplicationMiddleware<UpdateContext> { }
/// <summary>
/// 升级自身的Upgrade
/// </summary>
public class UpgradMiddleware : IUpgrad
{
    public async Task InvokeAsync(ApplicationDelegate<UpdateContext> next, UpdateContext context)
    {
        var sourcePath = context.AppPath;
        var patchPath = context.PatchPath;
        var upgradeName = context.UpgradName;
        upgradeName = Path.GetFileNameWithoutExtension(upgradeName);
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
            }
        }
        await next(context);
    }
}