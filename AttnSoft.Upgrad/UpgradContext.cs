﻿using AttnSoft.AutoUpdate;
using GeneralUpdate.Common.FileBasic;
using GeneralUpdate.Common.JsonContext;
using System;
using System.IO;
using System.Text.Json;

namespace AttnSoft.Upgrad;
public class UpgradContext
{
    public VersionInfo UpdateVersion { get; set; }
    public string AppPath { get; set; }
    public string BackupPath { get; set; }
    public string PatchPath { get; set; }
    internal UpgradContext()
    {
        var filename = "newVersionInfo.json";
        var curPath = AppDomain.CurrentDomain.BaseDirectory;
        filename= Path.Combine(curPath, filename);
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

        UpdateVersion = JsonSerializer.Deserialize<VersionInfo>(jsonString, VersionInfoJsonContext.Default.VersionInfo);
        if (UpdateVersion != null)
        {
            if(UpdateVersion.BlackFiles!= null && UpdateVersion.BlackFiles.Count > 0)
            {
                BlackListManager.Instance.AddBlackFiles(UpdateVersion.BlackFiles);
            }
            if (UpdateVersion.BlackFormats != null && UpdateVersion.BlackFormats.Count > 0)
            {
                BlackListManager.Instance.AddBlackFileFormats(UpdateVersion.BlackFormats);
            }
            if (UpdateVersion.SkipDirectorys != null && UpdateVersion.SkipDirectorys.Count > 0)
            {
                BlackListManager.Instance.AddSkipDirectorys(UpdateVersion.SkipDirectorys);
            }
        }
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
    }

}