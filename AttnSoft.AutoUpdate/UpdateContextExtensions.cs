using AttnSoft.AutoUpdate.Common;
using System;
using System.Collections.Generic;
using System.Net;

using System.Text;
using System.Threading.Tasks;

#if !NETFRAMEWORK
using System.Net.Http;
#endif

namespace AttnSoft.AutoUpdate;
public static partial class UpdateContextExtensions
{
    public static UpdateContext UseOss(this UpdateContext context)
    {
        context.OnGetUpdateVersionInfo = GetVersionInfoFromOss;
        return context;
    }
    internal async static Task<List<VersionInfo>?> GetVersionInfoFromOss(UpdateContext context)
    {
#if !NETFRAMEWORK
        using var httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (a, b, c, d) => true
        });

        using (HttpResponseMessage response = await httpClient.GetAsync(context.UpdateUrl, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();
            string responseJsonStr = await response.Content.ReadAsStringAsync();
            return DefaultJsonConvert.JsonConvert.Deserialize<List<VersionInfo>>(responseJsonStr);
        }
#else
        using (WebClient client = new WebClient())
        {
            var data= await client.DownloadDataTaskAsync(new Uri(context.UpdateUrl));
            if (data != null && data.Length > 0)
            {
                string responseJsonStr = Encoding.UTF8.GetString(data);
                return DefaultJsonConvert.JsonConvert.Deserialize<List<VersionInfo>>(responseJsonStr);
            }
        }
        return new List<VersionInfo>();
#endif

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
    internal static async Task<List<VersionInfo>?> GetVersionInfoFroWebApi(UpdateContext context)
    {
#if !NETFRAMEWORK
        var uri = new Uri(context.UpdateUrl);
        using var httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (a, b, c, d) => true
        });

        httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html, application/xhtml+xml, */*");
        //这里如果有权限认证,建议通过事件在主程序中实现
        //if (!string.IsNullOrEmpty(scheme) && !string.IsNullOrEmpty(token))
        //{
        //    httpClient.DefaultRequestHeaders.Authorization =
        //        new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, token);
        //}
        string postData = $"{{\"Version\": \"{context.ClientVersion}\", \"AppKey\": \"{context.AppSecretKey}\"}}";

        var stringContent = new StringContent(postData, Encoding.UTF8, "application/json");
        var result = await httpClient.PostAsync(uri, stringContent);
        var responseJsonStr = await result.Content.ReadAsStringAsync();
        //string responseJsonStr = "";
        return DefaultJsonConvert.JsonConvert.Deserialize<List<VersionInfo>>(responseJsonStr);
#else
        using (WebClient client = new WebClient())
        {
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            string postData = $"{{\"Version\": \"{context.ClientVersion}\", \"AppKey\": \"{context.AppSecretKey}\"}}";
            var data = await client.UploadDataTaskAsync(new Uri(context.UpdateUrl), "POST", Encoding.UTF8.GetBytes(postData));
            if (data != null && data.Length > 0)
            {
                string responseJsonStr = Encoding.UTF8.GetString(data);
                return DefaultJsonConvert.JsonConvert.Deserialize<List<VersionInfo>>(responseJsonStr);
            }
        }
        return new List<VersionInfo>();
#endif
    }

}
