using AttnSoft.AutoUpdate.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AttnSoft.AutoUpdate
{
    public class UpdateContext
    {
        /// <summary>
        /// 发现新版本事件
        /// </summary>
        public Action<ApplicationDelegate<UpdateContext>, UpdateContext>? OnFindNewVersion;

        //触发下载取消事件
        //public event Action? OnDownloadCanceled;
        //触发下载完成事件
        public Action? OnDownloadCompleted;
        //触发下载失败事件
        public Action<Exception>? OnDownloadException;
        //触发下载进度事件
        public Action<long, int>? OnDownloadProgressChanged;

        //触发更新完成事件
        public Action<UpdateContext>? OnUpdateCompleted;
        //触发更新异常事件
        public Action<Exception>? OnUpdateException;

        public IServiceCollection Services { get; }=new ServiceCollection();
        public IServiceProvider CreateServiceProvider()
        {
            return  Services.BuildServiceProvider();
        }
        public UpdateContext()
        {
            Services.AddSingleton<ICheckUpdate, CheckUpdateUseOSSMiddleware>();
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
        public string UpgradName { get; set; } = "Upgrad";

        string _startAppCmd;
        /// <summary>
        /// The name of the main application, without .exe.
        /// </summary>
        public string StartAppCmd
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

        string _clientVersion;

        /// <summary>
        /// Client current version.
        /// </summary>
        public string ClientVersion 
        { 
            get { return _clientVersion; } 
            set { _clientVersion = value; clientVer = null; } }
        Version? clientVer;
        public Version GetClientVersion()
        {
            if (clientVer == null)
            {
                if (!string.IsNullOrEmpty(ClientVersion))
                {
                    clientVer = new Version(ClientVersion);
                }
                else
                {
                    clientVer = Assembly.GetEntryAssembly().GetName().Version;
                }
            }
            return clientVer;
        }
        /// <summary>
        /// The main program update version.
        /// </summary>
        public VersionInfo UpdateVersion { get; set; }
        string _appPath;
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
        public string BackupPath { get; set; }
        public string PatchPath { get; set; }
        /// <summary>
        /// Download file temporary storage path (for update file logic).
        /// </summary>
        public string TempPath { get; set; }

        /// <summary>
        /// The fnull name of the compressed package file.
        /// </summary>
        public string ZipFileName { get; set; }

        /// <summary>
        /// API address for reporting update status.
        /// </summary>
        public string ReportUrl { get; set; }

        public string AppSecretKey { get; set; }

    }
}
