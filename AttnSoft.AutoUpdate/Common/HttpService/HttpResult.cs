/*
 * 描述：
 * 作者：LAW  
 * 电子邮箱：315204916@qq.com
 * 时间：2022-05-06 22:36:48
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace AttnSoft.AutoUpdate.Common
{
    public interface IHttpResult
    {
        HttpStatusCode StatusCode { get; set; }
        WebHeaderCollection Header { get; set; }
        string Cookie { get; set; }
        bool HasError { get; }
        string ErrorMessage { get; }
        string Content { get; set; }
        byte[] ResultByte { get; set; }
    }

    /// <summary>
    /// Http返回参数类
    /// </summary>
    public class HttpResult: IHttpResult
    {
        /// <summary>
        /// Http请求返回的Cookie
        /// </summary>
        public string Cookie { get; set; }
        /// <summary>
        /// Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection { get; set; }
        private string _html = string.Empty;
        /// <summary>
        /// 返回的String类型数据 只有ResultType.String时才返回数据，其它情况为空
        /// </summary>
        public string Content
        {
            get { return _html; }
            set { _html = value; }
        }
        /// <summary>
        /// 返回的Byte数组 只有ResultType.Byte时才返回数据，其它情况为空
        /// </summary>
        public byte[] ResultByte { get; set; }
        /// <summary>
        /// header对象
        /// </summary>
        public WebHeaderCollection Header { get; set; }
        /// <summary>
        /// 返回状态说明
        /// </summary>
        public string StatusDescription { get; set; }
        /// <summary>
        /// 返回状态码,默认为OK
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
        public bool HasError { get { return StatusCode != HttpStatusCode.OK; }}
        public string ErrorMessage { get { return HasError ? StatusDescription + "\n" + Content : String.Empty; } }
        /// <summary>
        /// 最后访问的URl
        /// </summary>
        public string ResponseUri { get; set; }
        /// <summary>
        /// 获取重定向的URl
        /// </summary>
        public string RedirectUrl
        {
            get
            {
                try
                {
                    if (Header != null && Header.Count > 0)
                    {
                        if (Header.AllKeys.Any(k => k.ToLower().Contains("location")))
                        {
                            string baseurl = Header["location"].ToString().Trim();
                            string locationurl = baseurl.ToLower();
                            if (!string.IsNullOrEmpty(locationurl))
                            {
                                bool b = locationurl.StartsWith("http://") || locationurl.StartsWith("https://");
                                if (!b)
                                {
                                    baseurl = new Uri(new Uri(ResponseUri), baseurl).AbsoluteUri;
                                }
                            }
                            return baseurl;
                        }
                    }
                }
                catch { }
                return string.Empty;
            }
        }
    }
}
