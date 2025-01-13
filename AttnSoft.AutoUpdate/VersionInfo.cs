using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AttnSoft.AutoUpdate;

public class VersionInfo
{
    [JsonPropertyName("recordId")]
    public int RecordId { get; set; } = 0;

    [JsonPropertyName("version")]
    public Version? Version { get; set; }

    [JsonPropertyName("releaseDate")]
    public DateTime? ReleaseDate { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("hash")]
    public string? Hash { get; set; }

    /// <summary>
    /// 更新后启动主程序的命令
    /// </summary>
    [JsonPropertyName("startAppCmd")]
    public string? StartAppCmd { get; set; }

    /// <summary>
    /// 升级需要的最低版本
    /// </summary>
    [JsonPropertyName("requiredMinVersion")]
    public string ? RequiredMinVersion { get; set; }

    /// <summary>
    /// 是否强制更新
    /// </summary>
    [JsonPropertyName("isForcibly")]
    public bool IsForcibly { get; set; }

    //[JsonPropertyName("packageSize")]
    //public long PackageSize { get; set; } = 0;

    /// <summary>
    /// 更新描述
    /// </summary>
    [JsonPropertyName("desc")]
    public string? Desc { get; set; }
    /// <summary>
    /// 更新日志地址
    /// </summary>
    [JsonPropertyName("updateLogUrl")]
    public string UpdateLogUrl { get; set; }

    [JsonPropertyName("blackFileFormats")]
    public List<string> BlackFormats { get; set; }=new List<string>();

    [JsonPropertyName("blackFiles")]
    public List<string> BlackFiles { get; set; }= new List<string>();

    [JsonPropertyName("skipDirectorys")]
    public List<string>? SkipDirectorys { get; set; } = new List<string>();

    //[XmlElement("productId")]
    //[JsonPropertyName("productId")]
    //public string? ProductId { get; set; }

    //[XmlElement("platform", typeof(int))]
    //[JsonPropertyName("platform")]
    //public int Platform { get; set; } = 0;

    //[JsonPropertyName("appType")]
    //public int? AppType { get; set; }

}