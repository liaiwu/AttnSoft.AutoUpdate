
using AttnSoft.AutoUpdate.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace AttnSoft.AutoUpdate;
public static partial class UpdateContextExtensions
{
    public static UpdateContext UseOssGetVersionInfo(this UpdateContext context)
    {
        context.OnGetUpdateVersionInfo += GetVersionInfoFromOss;
        return context;
    }
    internal async static Task<List<VersionInfo>?> GetVersionInfoFromOss(UpdateContext context)
    {
        using var httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = CheckValidationResult
        });

        using (HttpResponseMessage response = await httpClient.GetAsync(context.UpdateUrl, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();
            string responseJsonStr = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<VersionInfo>>(responseJsonStr);
        }

    }
    public static UpdateContext UseWebApiGetVersionInfo(this UpdateContext context)
    {
        context.OnGetUpdateVersionInfo += GetVersionInfoFroWebApi;
        return context;
    }
    internal static async Task<List<VersionInfo>?> GetVersionInfoFroWebApi(UpdateContext context)
    {

        var uri = new Uri(context.UpdateUrl);
        using var httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = CheckValidationResult
        });
        var timeout = httpClient.Timeout;
        //httpClient.Timeout = TimeSpan.FromSeconds(15);
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
        return JsonSerializer.Deserialize<List<VersionInfo>>(responseJsonStr);
    }
    private static bool CheckValidationResult(
    HttpRequestMessage? message,
    X509Certificate2? certificate,
    X509Chain? chain,
    SslPolicyErrors sslPolicyErrors
) => true;
}
