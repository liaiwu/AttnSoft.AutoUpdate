using System.Collections.Generic;
namespace AttnSoft.AutoUpdate.JsonContext;



#if !NETFRAMEWORK
using System.Text.Json.Serialization;

[JsonSerializable(typeof(VersionInfoList))]
public partial class VersionRespJsonContext : JsonSerializerContext
{
}
#endif


public class VersionInfoList : List<VersionInfo>
{
    public VersionInfoList()
    {
    }
    public VersionInfoList(IEnumerable<VersionInfo> collection) : base(collection)
    {
    }
    public VersionInfoList(int capacity) : base(capacity)
    {
    }
}


