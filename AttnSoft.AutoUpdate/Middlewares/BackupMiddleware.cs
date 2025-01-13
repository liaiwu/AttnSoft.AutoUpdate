using GeneralUpdate.Common.FileBasic;
using System.IO;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate.Middlewares;
/// <summary>
/// 备份主程序
/// </summary>
public interface IBackup : IApplicationMiddleware<UpdateContext> { }
/// <summary>
/// 备份中间件
/// </summary>
public class BackupMiddleware : IBackup
{
    public async Task InvokeAsync(ApplicationDelegate<UpdateContext> next, UpdateContext context)
    {
        context.BackupPath = Path.Combine(context.AppPath,
            $"{StorageManager.DirectoryName}{context.GetClientVersion()}");
        StorageManager.Backup(context.AppPath, context.BackupPath, BlackListManager.Instance.SkipDirectorys);

        await next(context);
    }
}