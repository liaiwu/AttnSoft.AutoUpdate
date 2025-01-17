using AttnSoft.AutoUpdate;
namespace GeneralUpdate.Common.JsonContext;

#if !NETFRAMEWORK
using System.Text.Json.Serialization;

[JsonSerializable(typeof(VersionInfo))]
public partial class VersionInfoJsonContext : JsonSerializerContext;

#endif