using AttnSoft.AutoUpdate.Common;
using System;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate.Middlewares;
/// <summary>
/// 升级包Hash校验
/// </summary>
public interface IHashCheck : IApplicationMiddleware<UpdateContext> { }
/// <summary>
/// 升级包Hash校验中间件
/// </summary>
public class HashCheckMiddleware : IHashCheck
{
    public async Task InvokeAsync(ApplicationDelegate<UpdateContext> next, UpdateContext context)
    {
        var hash = context.UpdateVersion.Hash;
        if (!string.IsNullOrEmpty(hash))
        {
            var path = context.ZipFileName;
            var isVerify = VerifyFileHash(path, hash);
            if (!isVerify)
            {
                context.OnUpdateException?.Invoke(new Exception("Hash verification failed ."));
                return;
            } 
        }
        await next(context);
    }

    private bool VerifyFileHash(string path, string hash)
    {
        var hashSha256 = HashAlgorithmService.ComputeHash(path);
        return string.Equals(hash, hashSha256, StringComparison.OrdinalIgnoreCase);
    }
}