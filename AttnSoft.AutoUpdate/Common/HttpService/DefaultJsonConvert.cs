/*
 * 描述：
 * 作者：LAW  
 * 电子邮箱：315204916@qq.com
 * 时间：2022-05-09 11:05:41
 */
using System;

#if NETSTANDARD || NETCOREAPP

using System.Text.Json;

namespace AttnSoft.AutoUpdate.Common
{
    public class DefaultJsonConvert : IJsonConvert
    {
        public JsonSerializerOptions Options { get; set; }
        public T Deserialize<T>(string jsonstr)
        {
            return JsonSerializer.Deserialize<T>(jsonstr, Options);
        }

        public string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, Options);
        }
    }
}

#else

using Newtonsoft.Json;
//using System.Web.Script.Serialization;
namespace AttnSoft.AutoUpdate.Common
{
    internal class DefaultJsonConvert : IJsonConvert
    {
        //JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        public T Deserialize<T>(string jsonstr)
        {
            T result;
            try
            {
                result=JsonConvert.DeserializeObject<T>(jsonstr);
                //result = javaScriptSerializer.Deserialize<T>(jsonstr);
            }
            catch (Exception)
            {
                result = default(T);
            }
            return result;
        }

        public string Serialize(object obj)
        {            
            string result;
            try
            {
                result=JsonConvert.SerializeObject(obj);
                //result = javaScriptSerializer.Serialize(obj);
            }
            catch (Exception)
            {
                result = string.Empty;
            }
            return result;
        }
    }
}
#endif
