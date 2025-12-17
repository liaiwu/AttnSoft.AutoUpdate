using AttnSoft.AutoUpdate.Common;
using GeneralUpdate.Common.FileBasic;
using System;
using System.IO;
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
        if(File.Exists(sourcePath) == false)
        {
            Console.WriteLine($"要解压的文件不存在:{sourcePath}");
            throw new FileNotFoundException("未找到下载的升级包文件!", sourcePath);
        }
        CompressionService.Decompress(sourcePath, patchPath);
#if DEBUG
        Console.WriteLine($"解压完成!sourcePath:{sourcePath},TempDirectory:{patchPath}");
        Console.ReadKey();
#endif
        await next(context);
    }
}