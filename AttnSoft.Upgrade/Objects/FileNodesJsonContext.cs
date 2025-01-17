using System.Collections.Generic;
using GeneralUpdate.Common.FileBasic;

namespace GeneralUpdate.Common.JsonContext;

#if !NETFRAMEWORK
using System.Text.Json.Serialization;
[JsonSerializable(typeof(List<FileNode>))]
public partial class FileNodesJsonContext : JsonSerializerContext;

#endif