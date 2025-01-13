/*
 * 描述：
 * 作者：LAW  
 * 电子邮箱：315204916@qq.com
 * 时间：2022-05-06 23:22:19
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace AttnSoft.AutoUpdate.Common
{
    public interface IHttpService
    {
        string ServerUrl { get; set; }
        string ContentType { get; set; }
        Encoding Encoding { get; set; }
        //WebHeaderCollection Header { get; }
        //IJsonConvert JsonConverter { get; set; }
        string Get(string path);
        string Get(string path, IDictionary<string, string>? queryParameter = null);
        string Get(string path, IDictionary<string,string>? header=null,IDictionary<string,string >? queryParameter=null);
        T Get<T>(string path, IDictionary<string, string>? header = null, IDictionary<string, string>? queryParameter = null);
        string Post(string path, object? body);
        string Post(string path, IDictionary<string, string>? queryParameter, object? body);
        string Post(string path, IDictionary<string,string>? header,IDictionary<string, string>? queryParameter,object? body);
        //string Get(string path, IDictionary<string, string> header = null, IDictionary<string, string> queryParameter = null, object body = null);
        //string Post(string path, IDictionary<string, string> header = null, IDictionary<string, object> queryParameter = null, object body = null);
        T Post<T>(string path, IDictionary<string, string>? header, IDictionary<string, string>? queryParameter, object? body);

        IHttpResult Request(string path, IDictionary<string, string> header, IDictionary<string, string> queryParameter, object body);
    }
}
