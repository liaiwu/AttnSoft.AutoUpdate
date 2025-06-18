using AttnSoft.AutoUpdate;
using AttnSoft.AutoUpdate.Common;
using AttnSoft.AutoUpdate.JsonContext;
using GeneralUpdate.Common.FileBasic;
using GeneralUpdate.Common.JsonContext;
using System;
using System.Collections.Generic;
using System.IO;

namespace AttnSoft.Upgrad;
public class UpgradContext
{
    public VersionInfo UpdateVersion { get; set; }
    public string AppPath { get; set; }
    public string BackupPath { get; set; }
    public string PatchPath { get; set; }
    internal UpgradContext()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (var index = 0; index < args.Length; index++)
        {
            string arg = args[index];
            switch (arg)
            {
                case "--appPath":
                    AppPath = args[index + 1];
                    break;
                case "--backupPath":
                    BackupPath = args[index + 1];
                    break;
                case "--patchPath":
                    PatchPath = args[index + 1];
                    break;
            }
        }
        if (string.IsNullOrEmpty(AppPath))
        {
            throw new ArgumentNullException("AppPath is null or empty!");
        }
        if (string.IsNullOrEmpty(BackupPath))
        {
            throw new ArgumentNullException("BackupPath is null or empty!");
        }
        if (string.IsNullOrEmpty(PatchPath))
        {
            throw new ArgumentNullException("PatchPath is null or empty!");
        }

        var filename = "newVersionInfo.json";
        filename= Path.Combine(AppPath, filename);
        string jsonString = string.Empty;
        if (File.Exists(filename))
        { jsonString = File.ReadAllText(filename); }
        else
        {
            throw new FileNotFoundException("newVersionInfo.json not found!");
        }
        if(string.IsNullOrEmpty(jsonString))
        {
            throw new ArgumentNullException("newVersionInfo.json is null or empty!");
        }
#if NETFRAMEWORK
        UpdateVersion = DefaultJsonConvert.JsonConvert.Deserialize<VersionInfo>(jsonString);
#else
        UpdateVersion = DefaultJsonConvert.JsonConvert.Deserialize<VersionInfo>(jsonString, VersionInfoJsonContext.Default.VersionInfo);
#endif
        if (UpdateVersion != null)
        {
            if(UpdateVersion.BlackFiles!= null && UpdateVersion.BlackFiles.Length > 0)
            {
                BlackListManager.Instance.AddBlackFiles(new List<string>(UpdateVersion.BlackFiles));
            }
            if (UpdateVersion.BlackFormats != null && UpdateVersion.BlackFormats.Length > 0)
            {
                BlackListManager.Instance.AddBlackFileFormats(new List<string>(UpdateVersion.BlackFormats));
            }
            if (UpdateVersion.SkipDirectorys != null && UpdateVersion.SkipDirectorys.Length > 0)
            {
                BlackListManager.Instance.AddSkipDirectorys(new List<string>(UpdateVersion.SkipDirectorys));
            }
        }

    }

}
