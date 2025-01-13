/*
 * 描述：
 * 作者：LAW  
 * 邮箱：315204916@qq.com
 * 时间：2022-05-09 9:04:21
 */
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


namespace AttnSoft.AutoUpdate.Common
{
    public class HttpFactory
    {
        public static IJsonConvert JsonConverter = new DefaultJsonConvert();

        static System.Collections.Concurrent.ConcurrentDictionary<string, IHttpService> cache = new System.Collections.Concurrent.ConcurrentDictionary<string, IHttpService>();
        public static IHttpService GetHttpService(string url)
        {
            if (cache.TryGetValue(url, out var httpService))
            {
                return httpService;
            }
            else
            {
                CookieContainer cookieContainer = new CookieContainer();
                HttpClientHandler httpClientHandler= GetHttpClientHandler(cookieContainer, HttpClientWebProxy, DecompressionMethods.GZip);
                HttpClient client = new HttpClient(httpClientHandler);

                HttpClientService service = new HttpClientService(client, url);
                service.CookieContainer = cookieContainer;
                cache[url] = service;
                service.JsonConverter = JsonConverter;
                return service;
            }
        }
        /// <summary>
        /// 获取 HttpClientHandler 对象
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <param name="webProxy"></param>
        /// <returns></returns>
        public static HttpClientHandler GetHttpClientHandler(CookieContainer cookieContainer = null, IWebProxy webProxy = null, DecompressionMethods decompressionMethods = DecompressionMethods.None)
        {
            var httpClientHandler = new HttpClientHandler()
            {
                UseProxy = webProxy != null,
                Proxy = webProxy,
                UseCookies = cookieContainer != null,
                AllowAutoRedirect = true,
                ServerCertificateCustomValidationCallback = CheckValidationResult,
                //CookieContainer = cookieContainer,//如果为null，赋值的时候会出现异常
                AutomaticDecompression = decompressionMethods
            };

            if (cookieContainer != null)
            {
                httpClientHandler.CookieContainer = cookieContainer;
            }
            return httpClientHandler;
        }
        private static bool CheckValidationResult(HttpRequestMessage message,
            X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
        public void ClearCache()
        {
            cache.Clear();
        }
        /// <summary>
        /// 作用于 SenparcHttpClient 的 WebProxy（需要在 AddSenparcGlobalServices 之前定义）
        /// </summary>
        public static IWebProxy HttpClientWebProxy { get; set; } = null;

        ///// <summary>
        ///// 设置Web代理
        ///// </summary>
        ///// <param name="host"></param>
        ///// <param name="port"></param>
        ///// <param name="username"></param>
        ///// <param name="password"></param>
        //public static void SetHttpProxy(string host, int port, string username, string password)
        //{
        //    ICredentials cred = new NetworkCredential(username, password);
        //    if (!string.IsNullOrEmpty(host))
        //    {
        //        HttpClientWebProxy = new CoreWebProxy(new Uri(host + ":" + port ?? "80"), cred);
        //    }
        //}
    }
}
