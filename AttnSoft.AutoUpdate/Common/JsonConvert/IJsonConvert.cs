using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if !NETFRAMEWORK
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
#endif

namespace AttnSoft.AutoUpdate.Common
{
    public interface IJsonConvert
    {
        string Serialize(object obj);
        T? Deserialize<T>(string jsonstr);
#if !NETFRAMEWORK
        string Serialize<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo);
        T? Deserialize<T>(string jsonstr, JsonTypeInfo<T>? typeInfo = null);
#endif
    }

}