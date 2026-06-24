using System;
using System.Text.Json.Serialization;

namespace AttnSoft.AutoUpdate;

public class VersionInfo
{
    [JsonPropertyName("RecordId")]
    public int RecordId { get; set; } = 0;

    [JsonPropertyName("Version")]
    public Version? Version { get; set; }

    [JsonPropertyName("ReleaseDate")]
    public DateTime? ReleaseDate { get; set; }

    [JsonPropertyName("Url")]
    public string Url { get; set; }

    [JsonPropertyName("Hash")]
    public string? Hash { get; set; }

    /// <summary>
    /// 更新后启动主程序的命令
    /// </summary>
    [JsonPropertyName("StartAppCmd")]
    public string? StartAppCmd { get; set; }

    /// <summary>
    /// 升级需要的最低版本
    /// </summary>
    [JsonPropertyName("RequiredMinVersion")]
    public string? RequiredMinVersion { get; set; }

    /// <summary>
    /// 是否强制更新
    /// </summary>
    [JsonPropertyName("IsForcibly")]
    public bool IsForcibly { get; set; }

    /// <summary>
    /// 是否可以跳过
    /// </summary>
    [JsonPropertyName("CanSkip")]
    public bool CanSkip { get; set; }

    /// <summary>
    /// 更新描述
    /// </summary>
    [JsonPropertyName("Desc")]
    public string? Desc { get; set; }

    /// <summary>
    /// 更新日志地址
    /// </summary>
    [JsonPropertyName("UpdateLogUrl")]
    public string UpdateLogUrl { get; set; }

    [JsonPropertyName("BlackFormats")]
    public string[] BlackFormats { get; set; }

    [JsonPropertyName("BlackFiles")]
    public string[] BlackFiles { get; set; }

    [JsonPropertyName("SkipDirectorys")]
    public string[] SkipDirectorys { get; set; }

    [JsonPropertyName("SkipVersion")]
    public Version? SkipVersion { get; set; }
}