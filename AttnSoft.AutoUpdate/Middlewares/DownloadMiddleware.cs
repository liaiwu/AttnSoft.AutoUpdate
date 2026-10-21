using AttnSoft.AutoUpdate.Common;
using GeneralUpdate.Common.FileBasic;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate.Middlewares;
/// <summary>
/// 下载升级包
/// </summary>
public interface IDownload : IApplicationMiddleware<UpdateContext> { }
/// <summary>
/// 下载升级包中间件
/// </summary>
public class DownloadMiddleware : IDownload
{
    public async Task InvokeAsync(ApplicationDelegate<UpdateContext> next, UpdateContext context)
    {
        string url = context.UpdateVersion.Url;
        string PackageName = ResolvePackageName(url);

        context.TempPath = StorageManager.GetTempDirectory("main_temp");

        UpdateLog.Debug($"TempPath:{context.TempPath}");

        var fileFullName = Path.Combine(context.TempPath, PackageName);
        context.ZipFileName = fileFullName;

        FileDownloader downloader = new FileDownloader();
        downloader.DownloadStallTimeoutMs = context.DownloadStallTimeoutMs;
        downloader.OnProgressChanged += (arg1, arg2) => context.OnDownloadProgressChanged?.Invoke(arg1, arg2);
        downloader.OnDownloadCompleted +=() => context.OnDownloadCompleted?.Invoke();
        downloader.OnDownloadException+=(ex) => context.OnDownloadException?.Invoke(ex);

        try
        {
            await downloader.DownloadFileAsync(url, fileFullName).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // 绝不能继续 next：半个升级包会被当成有效内容一路解压、安装下去。
            UpdateLog.Error("下载升级包失败", ex);
            context.OnUpdateException?.Invoke(ex);
            return;
        }

        await next(context).ConfigureAwait(false);
    }

    // URL 的 query 与 fragment 不是文件名的一部分：带 token 的签名链接（OSS / S3 / CDN 鉴权）
    // 直接用 Path.GetFileName 会把 "?Expires=..." 整段带进文件名，Windows 判为非法路径。
    private static string ResolvePackageName(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "update_package.zip";

        var name = Uri.TryCreate(url, UriKind.Absolute, out var uri)
            ? Path.GetFileName(Uri.UnescapeDataString(uri.AbsolutePath))
            : Path.GetFileName(url);

        return string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            ? "update_package.zip"
            : name;
    }
}
