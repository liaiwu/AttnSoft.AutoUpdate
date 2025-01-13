using AttnSoft.AutoUpdate;
using System.Text.Json.Serialization;

namespace GeneralUpdate.Common.JsonContext;

[JsonSerializable(typeof(VersionInfo))]
public partial class VersionInfoJsonContext : JsonSerializerContext;