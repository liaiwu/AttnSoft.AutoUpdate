using System;
using System.Collections.Generic;

namespace AttnSoft.AutoUpdate;

#if NETFRAMEWORK

public class VersionInfo
{
    public int RecordId { get; set; } = 0;

    public Version? Version { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public string Url { get; set; }

    public string? Hash { get; set; }

    /// <summary>
    /// 更新后启动主程序的命令
    /// </summary>
    public string? StartAppCmd { get; set; }

    /// <summary>
    /// 升级需要的最低版本
    /// </summary>
    public string ? RequiredMinVersion { get; set; }

    /// <summary>
    /// 是否强制更新
    /// </summary>
    public bool IsForcibly { get; set; }

    /// <summary>
    /// 是否可以跳过
    /// </summary>
    public bool CanSkip { get; set; }

    /// <summary>
    /// 更新描述
    /// </summary>
    public string? Desc { get; set; }
    /// <summary>
    /// 更新日志地址
    /// </summary>
    public string UpdateLogUrl { get; set; }


    public string[] BlackFormats { get; set; }

    public string[] BlackFiles { get; set; }

    public string[]? SkipDirectorys { get; set; }
    //跳过的版本
    public Version? SkipVersion { get; set; }

}
#else
using System.Text.Json.Serialization;

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

    //[JsonPropertyName("packageSize")]
    //public long PackageSize { get; set; } = 0;

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
    //跳过的版本

    [JsonPropertyName("SkipVersion")]
    public Version? SkipVersion { get; set; }

}
#endif