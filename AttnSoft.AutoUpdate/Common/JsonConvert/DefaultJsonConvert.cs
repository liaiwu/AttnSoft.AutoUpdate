
#if NETFRAMEWORK

using Newtonsoft.Json;

using System;
//using System.Web.Script.Serialization;
namespace AttnSoft.AutoUpdate.Common
{
    internal class DefaultJsonConvert : IJsonConvert
    {
        public JsonSerializerSettings Settings { get; private set; } = new JsonSerializerSettings();
        private DefaultJsonConvert() 
        {
            Settings.Converters.Add(new VersionConverter());
        }
        public static IJsonConvert JsonConvert { get; set; } = new DefaultJsonConvert();

        public T? Deserialize<T>(string jsonstr)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonstr, Settings);
        }
        public string Serialize(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Settings);
        }
    }
}
public class VersionConverter : JsonConverter<Version>
{
    // 序列化：将 Version 对象转换为字符串
    public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString()); // 将 Version 转换为字符串格式（如 "1.2.3.4"）
    }

    // 反序列化：将字符串转换为 Version 对象
    public override Version ReadJson(JsonReader reader, Type objectType, Version existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var versionString = reader.Value as string; // 读取 JSON 中的字符串值
        if (string.IsNullOrEmpty(versionString))
        {
            return null; // 如果字符串为空，返回 null
        }

        return new Version(versionString); // 将字符串解析为 Version 对象
    }
}
#else

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace AttnSoft.AutoUpdate.Common
{
    internal class DefaultJsonConvert : IJsonConvert
    {
        private DefaultJsonConvert() { }

        public static IJsonConvert JsonConvert { get; set; }= new DefaultJsonConvert();

        public JsonSerializerOptions Options { get; set; } = new JsonSerializerOptions { PropertyNamingPolicy=null };
        public T? Deserialize<T>(string jsonstr)
        {
            return JsonSerializer.Deserialize<T>(jsonstr, Options);
        }

        public T? Deserialize<T>(string jsonstr, JsonTypeInfo<T>? typeInfo = null)
        {
            if (typeInfo != null)
            {
                return JsonSerializer.Deserialize(jsonstr, typeInfo);
            }
            return JsonSerializer.Deserialize<T>(jsonstr);
        }

        public string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj,Options);
        }

        public string Serialize<TValue>(TValue obj, JsonTypeInfo<TValue> typeInfo)
        {
           return  typeInfo != null ? JsonSerializer.Serialize(obj, typeInfo) : JsonSerializer.Serialize(obj, Options);
        }
    }
}

#endif
