using AttnSoft.AutoUpdate.Common;
using GeneralUpdate.Common.FileBasic;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate.Middlewares;
/// <summary>
/// 解压升级包
/// </summary>
public interface IDecompress : IApplicationMiddleware<UpdateContext> { }
/// <summary>
/// 解压升级包中间件
/// </summary>
public class DecompressMiddleware : IDecompress
{
    public async Task InvokeAsync(ApplicationDelegate<UpdateContext> next, UpdateContext context)
    {
        var sourcePath = context.ZipFileName;
        var patchPath = context.PatchPath = StorageManager.GetTempDirectory();

        CompressionService.Decompress(sourcePath, patchPath);
        await next(context);
    }
}