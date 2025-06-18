using AttnSoft.AutoUpdate;
namespace AttnSoft.AutoUpdate.JsonContext;

#if !NETFRAMEWORK
using System.Text.Json.Serialization;

[JsonSerializable(typeof(VersionInfo))]
public partial class VersionInfoJsonContext : JsonSerializerContext;

#endif