using AttnSoft.AutoUpdate.Middlewares;
using GeneralUpdate.Common.JsonContext;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate
{
    ///// <summary>
    ///// 表示可以处理应用请求的委托
    ///// </summary>
    ///// <typeparam name="TContext">中间件上下文类型</typeparam>
    ///// <param name="context">中间件上下文</param>
    ///// <returns></returns>
    //public delegate Task ApplicationDelegate<TContext>(TContext context);

    public class UpdateContext
    {

        public Func<UpdateContext, Task<List<VersionInfo>?>>? OnGetUpdateVersionInfo;
        /// <summary>
        /// 发现新版本事件
        /// </summary>
        public Func<ApplicationDelegate<UpdateContext>, UpdateContext,Task>? OnFindNewVersion;

        //触发下载取消事件
        //public event Action? OnDownloadCanceled;
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

        public IServiceCollection Services { get; } = new ServiceCollection();
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
        public string UpgradName { get; set; } = "AttnSoft.Upgrade";

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
                    var filename = "Version.json";
                    filename = Path.Combine(AppPath, filename);
                    string jsonString = string.Empty;
                    if (File.Exists(filename))
                    {
                        jsonString = File.ReadAllText(filename);
                        var localVer = JsonSerializer.Deserialize<VersionInfo>(jsonString, VersionInfoJsonContext.Default.VersionInfo);
                        if (localVer != null)
                        {
                            clientVer = localVer.Version;
                        }
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
