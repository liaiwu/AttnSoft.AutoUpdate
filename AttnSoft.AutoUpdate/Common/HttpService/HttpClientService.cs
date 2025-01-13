/*
 * 描述：
 * 作者：LAW  
 * 电子邮箱：315204916@qq.com
 * 时间：2022-05-06 23:41:59
 */
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http.Headers;
using System.Net.Http;

namespace AttnSoft.AutoUpdate.Common
{
    internal class HttpClientService : IHttpService
    {
        HttpClient client;
        const string _UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36";

        string _contentType = "application/json";
        public string ContentType { get { return _contentType; } set { _contentType = value; } }
        Encoding _encoding = Encoding.UTF8;
        public Encoding Encoding { get { return _encoding; } set { _encoding = value; } }
        CookieContainer cookieContainer = new CookieContainer();
        public CookieContainer CookieContainer { get { return cookieContainer; } set { cookieContainer = value; } }
        internal HttpClientService(HttpClient client, string serverUrl)
        {
            ServerUrl = serverUrl;
            this.client = client;
        }

        private string serverUrl;
        public string ServerUrl
        {
            get { return serverUrl; }
            set
            {
                serverUrl = value;
                if (serverUrl.EndsWith("/"))
                {
                    serverUrl = serverUrl.Remove(serverUrl.Length - 1, 1);
                }
            }
        }
        public IJsonConvert JsonConverter { get; set; }
        public string Get(string path)
        {
            IHttpResult result = Request("GET", path, null, null);
            return result.Content;
        }
        public string Get(string path, IDictionary<string, string> queryParameter = null)
        {
            IHttpResult result = Request("GET", path, null, queryParameter);
            return result.Content;
        }
        public string Get(string path, IDictionary<string, string> header = null, IDictionary<string, string> queryParameter = null)
        {
            IHttpResult result = Request("GET", path, header, queryParameter);
            return result.Content;
        }
        public T Get<T>(string path, IDictionary<string, string> header = null, IDictionary<string, string> queryParameter = null)
        {
            IHttpResult result = Request("GET", path, header, queryParameter);
            return JsonConverter.Deserialize<T>(result.Content);
        }
        public string Post(string path, object body = null)
        {
            IHttpResult result = Request("POST", path, null, null, body);
            return result.Content;
        }
        public string Post(string path, IDictionary<string, string> queryParameter = null, object body = null)
        {
            IHttpResult result = Request("POST", path, null, queryParameter, body);
            return result.Content;
        }
        public string Post(string path, IDictionary<string, string> header = null, IDictionary<string, string> queryParameter = null, object body = null)
        {
            IHttpResult result = Request("POST", path, header, queryParameter, body);
            return result.Content;
        }
        public T Post<T>(string path, IDictionary<string, string> header = null, IDictionary<string, string> queryParameter = null, object body = null)
        {
            IHttpResult result = Request("POST", path, header, queryParameter, body);
            return JsonConverter.Deserialize<T>(result.Content);
        }

        public IHttpResult Request(string path, IDictionary<string, string> header = null, IDictionary<string, string> queryParameter = null, object body = null)
        {
            string Method = body == null ? "GET" : "POST";
            return Request(Method, path, header, queryParameter, body);
        }
        private IHttpResult Request(string method, string path, IDictionary<string, string> header = null, IDictionary<string, string> queryParameter = null, object body = null)
        {
            if (!string.IsNullOrEmpty(path) && !path.StartsWith("/"))
            {
                path = "/" + path;
            }
            string url = ServerUrl + path;
            if (queryParameter != null)
            {
                string request = BuildRequest(queryParameter);
                if (path.IndexOf("?") != -1)
                {
                    url = url + "&" + request;
                }
                else
                {
                    url = url + "?" + request;
                }
            }
            string contentType = null;

            if (header == null)
            {
                header = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                header["KeepAlive"] = "false";
            }
            header["User-Agent"] = _UserAgent;

            client.DefaultRequestHeaders.Clear();
            foreach (var keyEntity in header)
            {
                if ("Content-Type".Equals(keyEntity.Key, StringComparison.OrdinalIgnoreCase))
                {
                    contentType = keyEntity.Value;
                    continue;
                }
                client.DefaultRequestHeaders.Add(keyEntity.Key, keyEntity.Value);
            }
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));

            if (contentType == null) contentType = _contentType;

            Uri uri = new Uri(url);
            var cookieHeader = cookieContainer.GetCookieHeader(uri);
            client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            string bodyData = string.Empty;
            if (body != null)
            {
                if (body is string)
                {
                    bodyData = (string)body;
                }
                else
                {
                    bodyData = JsonConverter.Serialize(body);
                }
            }
            HttpResult result = new HttpResult();
            HttpResponseMessage response=null;
            try
            {
                if ("GET".Equals(method, StringComparison.OrdinalIgnoreCase))
                {
                    client.DefaultRequestHeaders.Add("Method", "Get");
                    response = client.GetAsync(url).GetAwaiter().GetResult();//获取响应信息
                }
                else
                {

                    client.DefaultRequestHeaders.Add("Method", "Post");
                    HttpContent content = new StringContent(bodyData);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                    response = client.PostAsync(url, content).Result;
                }
            }
            catch (Exception ex)
            {
                result.StatusCode= HttpStatusCode.BadRequest;
                result.Content = ex.Message+"\n\r"+ex.InnerException;
                return result;
            }
            if (response != null)
            {
                result.StatusCode = response.StatusCode;
                result.StatusDescription = response.ReasonPhrase;
                if (response.Content.Headers.ContentType != null &&
                    response.Content.Headers.ContentType.CharSet != null &&
                    response.Content.Headers.ContentType.CharSet.ToLower().Contains("utf8"))
                {
                    response.Content.Headers.ContentType.CharSet = "utf-8";
                }
                result.Content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    SetResponseCookieContainer(cookieContainer, response);
                }
            }
            return result;
        }
        private static string BuildRequest(IDictionary<string, string> queryParameter)
        {
            StringBuilder urlBd = new StringBuilder();
            foreach (var item in queryParameter)
            {
                urlBd.Append(item.Key);
                urlBd.Append("=");
                urlBd.Append(item.Value == null ? string.Empty : Uri.EscapeDataString(item.Value));
                urlBd.Append("&");
            }
            if (urlBd.Length > 0)
            {
                urlBd.Length = urlBd.Length - 1;
            }
            return urlBd.ToString();
        }

        /// <summary>
        /// 从 Response 中设置 Cookie 到 CookieContainer
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <param name="response"></param>
        private static void SetResponseCookieContainer(CookieContainer cookieContainer, HttpResponseMessage response)
        {
            if (cookieContainer == null || response == null)
            {
                return;
            }
            IEnumerable<string> setCookieHeaders = null;
            if (response.Headers != null && response.Headers.TryGetValues("set-cookie", out setCookieHeaders))
            {
                if (setCookieHeaders == null)
                {
                    return;
                }
                foreach (var header in setCookieHeaders)
                {
                    cookieContainer.SetCookies(response.RequestMessage.RequestUri, header);
                }
            }

        }

    }
}
