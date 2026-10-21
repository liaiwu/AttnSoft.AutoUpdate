
using AttnSoft.AutoUpdate.Common;
using AttnSoft.AutoUpdate.JsonContext;

using System;
using System.Text;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate;
public static partial class UpdateContextExtensions
{
    public static UpdateContext UseOss(this UpdateContext context)
    {
        context.OnGetUpdateVersionInfo = GetVersionInfoFromOss;
        return context;
    }
    public async static Task<VersionInfoList?> GetVersionInfoFromOss(UpdateContext context)
    {
        WarnIfInsecureTransport(context.UpdateUrl);
        try
        {
            byte[] data = await CreateDownloader(context).GetDataAsync(context.UpdateUrl).ConfigureAwait(false);
            if (data.Length == 0) return null;
            string responseJsonStr = Encoding.UTF8.GetString(data);
#if NETFRAMEWORK
            return DefaultJsonConvert.JsonConvert.Deserialize<VersionInfoList>(responseJsonStr);
#else
            return DefaultJsonConvert.JsonConvert.Deserialize<VersionInfoList>(responseJsonStr, VersionRespJsonContext.Default.VersionInfoList);
#endif
        }
        catch (Exception ex)
        {
            // 失败一律返回 null 而不抛出：宿主靠 null 走备用 URL 兜底（AttnSoft.WinStart 实测依赖此语义）。
            // 这里不触发 OnUpdateException——CheckUpdateMiddleware 的 null 分支会触发一次，再触发就是两次。
            UpdateLog.Error("取版本信息失败", ex);
            return null;
        }
    }
    /// <summary>
    /// 通过WebApi方式获取服务器的版本信息
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static UpdateContext UseWebApi(this UpdateContext context)
    {
        context.OnGetUpdateVersionInfo = GetVersionInfoFroWebApi;
        return context;
    }
    public static async Task<VersionInfoList?> GetVersionInfoFroWebApi(UpdateContext context)
    {
        WarnIfInsecureTransport(context.UpdateUrl);
        try
        {
            string postData = $"{{\"Version\": \"{context.ClientVersion}\", \"AppKey\": \"{context.AppSecretKey}\"}}";
            // contentType 带 charset：改造前 net6+ 用 StringContent(..., Encoding.UTF8, ...) 出来的是
            // "application/json; charset=utf-8"，只写 "application/json" 会让按精确 media type 匹配的服务端回 415。
            byte[] data = await CreateDownloader(context).PostDataAsync(context.UpdateUrl,
                    Encoding.UTF8.GetBytes(postData), "application/json; charset=utf-8", "text/html, application/xhtml+xml, */*")
                .ConfigureAwait(false);
            if (data.Length == 0) return null;
            string responseJsonStr = Encoding.UTF8.GetString(data);
            UpdateLog.Debug($"从WebApi获得Json:{responseJsonStr}");
#if NETFRAMEWORK
            return DefaultJsonConvert.JsonConvert.Deserialize<VersionInfoList>(responseJsonStr);
#else
            return DefaultJsonConvert.JsonConvert.Deserialize<VersionInfoList>(responseJsonStr, VersionRespJsonContext.Default.VersionInfoList);
#endif
        }
        catch (Exception ex)
        {
            UpdateLog.Error("取版本信息失败", ex);
            return null;
        }
    }

    // 超时口径与下载升级包共用 DownloadStallTimeoutMs：版本信息只有几 KB，
    // 「连续 N 毫秒收不到数据」与「N 毫秒内没完成」观测上等价，不必再加一个开关。
    private static FileDownloader CreateDownloader(UpdateContext context)
        => new FileDownloader { DownloadStallTimeoutMs = context.DownloadStallTimeoutMs };

    // 禁用服务器证书校验是本组件的既有默认，用户必须知情。警告挂在 URL scheme 上而不是回调里：
    // 回调只在 TLS 握手时触发，http:// 下永不执行，挂在那里等于永远不响。
    private static void WarnIfInsecureTransport(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        var isHttps = Uri.TryCreate(url, UriKind.Absolute, out var uri)
                      && string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

        UpdateLog.Warning(isHttps
            ? "组件已禁用服务器证书校验，HTTPS 下仍可被中间人伪造证书。"
            : $"更新地址未使用 HTTPS（{url}），版本信息与升级包在传输中可被篡改；组件也已禁用证书校验。");
    }

}
