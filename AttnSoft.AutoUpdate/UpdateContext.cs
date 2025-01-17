using AttnSoft.AutoUpdate.Common;
using AttnSoft.AutoUpdate.Middlewares;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

#if !NETFRAMEWORK
using Microsoft.Extensions.DependencyInjection;
#endif

namespace AttnSoft.AutoUpdate
{
    public class UpdateContext
    {

        public Func<UpdateContext, Task<List<VersionInfo>?>>? OnGetUpdateVersionInfo;
        /// <summary>
        /// 发现新版本事件
        /// </summary>
        public Func<ApplicationDelegate<UpdateContext>, UpdateContext,Task>? OnFindNewVersion;

        //触发下载完成事件
        public Action? OnDownloadCompleted;
        //触发下载失败事件
        public Action<Exception>? OnDownloadException;
        //触发下载进度事件
        public Action<long, int>? OnDownloadProgressChanged;
        //触发更新异常事件
        public Action<Exception>? OnUpdateException;

        //触发更新完成事件
        public Action<UpdateContext>? OnUpdateCompleted;

        public ApplicationServiceCollection Services { get; } = new ApplicationServiceCollection();
        public IServiceProvider CreateServiceProvider()
        {
            return Services.BuildServiceProvider();
        }
        public UpdateContext()
        {
            Services.AddSingleton<ICheckCompletion, CheckCompletionMiddleware>();
            Services.AddSingleton<ICheckUpdate, CheckUpdateMiddleware>();
            Services.AddSingleton<IBackup, BackupMiddleware>();
            Services.AddSingleton<IDownload, DownloadMiddleware>();
            Services.AddSingleton<IHashCheck, HashCheckMiddleware>();
            Services.AddSingleton<IDecompress, DecompressMiddleware>();
            Services.AddSingleton<IUpgrad, UpgradMiddleware>();
            Services.AddSingleton<IStartUpgrad, StartUpgradMiddleware>();
        }
        /// <summary>
        /// Update check api address.
        /// </summary>
        public string UpdateUrl { get; set; }

        /// <summary>
        /// Need to start the name of the app.
        /// </summary>
        public string UpgradName { get; set; } = "Upgrade";

        string? _startAppCmd;
        /// <summary>
        /// The name of the main application, without .exe.
        /// </summary>
        public string? StartAppCmd
        {
            get
            {
                if (string.IsNullOrEmpty(_startAppCmd))
                {
                    var processModule = Process.GetCurrentProcess().MainModule;
                    _startAppCmd = Path.GetFileName(processModule.FileName);
                }
                return _startAppCmd;
            }
            set { _startAppCmd = value; }
        }

        /// <summary>
        /// Whether the main application needs to be updated.
        /// </summary>
        public bool IsMainUpdate { get; set; }

        Version clientVer;
        public Version ClientVersion
        {
            get
            {
                if (clientVer == null)
                {
                    var localVer = LocalVerManager.GetVersionInfo();
                    if (localVer!=null && localVer.Version!=null)
                    {
                        clientVer = localVer.Version;
                    }
                    else
                    {
                        clientVer = Assembly.GetEntryAssembly().GetName().Version;
                    }
                }
                return clientVer;
            }
            set
            {
                clientVer = value;
            }
        }
        LocalVerManager _LocalVerManager;
        public LocalVerManager LocalVerManager
        {
            get
            {
                if (_LocalVerManager == null)
                {
                    _LocalVerManager = new LocalVerManager(AppPath);
                }
                return _LocalVerManager;
            }
        }
        /// <summary>
        /// 跳过当前版本
        /// </summary>
        public void SkipVersion()
        {
            var localVer = LocalVerManager.GetVersionInfo();
            if (localVer == null)
            {
                localVer = new VersionInfo();
                localVer.Version = ClientVersion;
            }
            localVer.SkipVersion = UpdateVersion.Version;
            LocalVerManager.SaveVersionInfo(localVer);
        }
        /// <summary>
        /// The main program update version.
        /// </summary>
        public VersionInfo UpdateVersion { get; set; }
        string? _appPath;
        public string AppPath
        {
            get
            {
                if (string.IsNullOrEmpty(_appPath))
                {
                    var processModule = Process.GetCurrentProcess().MainModule;
                    _appPath = Path.GetDirectoryName(processModule.FileName);
                }
                return _appPath;
            }
            set { _appPath = value; }
        }
        public string? BackupPath { get; set; }
        public string? PatchPath { get; set; }
        /// <summary>
        /// Download file temporary storage path (for update file logic).
        /// </summary>
        public string? TempPath { get; set; }

        /// <summary>
        /// The fnull name of the compressed package file.
        /// </summary>
        public string? ZipFileName { get; set; }

        /// <summary>
        /// API address for reporting update status.
        /// </summary>
        public string? ReportUrl { get; set; }

        public string? AppSecretKey { get; set; }

    }
}
