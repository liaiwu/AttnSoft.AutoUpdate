using AttnSoft.AutoUpdate.Common;
using GeneralUpdate.Common.FileBasic;
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
        string PackageName = Path.GetFileName(url);

        context.TempPath = StorageManager.GetTempDirectory("main_temp");

        var fileFullName = Path.Combine(context.TempPath, PackageName);
        context.ZipFileName = fileFullName;

        FileDownloader downloader = new FileDownloader();
        downloader.OnProgressChanged += (arg1, arg2) => context.OnDownloadProgressChanged?.Invoke(arg1, arg2);
        downloader.OnDownloadCompleted +=() => context.OnDownloadCompleted?.Invoke();
        downloader.OnDownloadException+=(ex) => context.OnDownloadException?.Invoke(ex);

        //var cancellationTokenSource = new CancellationTokenSource();
        //await downloader.DownloadFileAsync(url, fileFullName, cancellationTokenSource.Token);

        await downloader.DownloadFileAsync(url, fileFullName);
        await next(context);
    }

}
