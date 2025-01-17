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
        var sourcePath = context.AppPath;
        var patchPath = context.PatchPath;
        var upgradeName = context.UpgradName;

#if NETFRAMEWORK
        string resUpgradeName = "AttnSoft.AutoUpdate.Upgrade.Framework.Upgrade.exe";
#else
        string resUpgradeName = "AttnSoft.AutoUpdate.Upgrade.AOT.Upgrade.exe";
#endif
        System.Reflection.Assembly app = System.Reflection.Assembly.GetExecutingAssembly();
        Stream? fsOpen = app.GetManifestResourceStream(resUpgradeName);
        if (fsOpen != null)
        {
            context.UpgradName = "Upgrade.exe";
            var filename = Path.Combine(sourcePath, context.UpgradName);
            FileStream fsSave = new FileStream(filename, FileMode.Create);
            int readcount = 0;
            byte[] bytes = new byte[1024];
            while ((readcount = fsOpen.Read(bytes, 0, 1024)) > 0)
            {
                fsSave.Write(bytes, 0, readcount);
            }
            fsSave.Close();
            fsOpen.Close();
        }
        else
        {
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
        }
        await next(context);
    }
}