using AttnSoft.AutoUpdate.Middlewares;
using GeneralUpdate.Common.Shared.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate
{
    //自动更新核心类
    public class UpdateApp : ApplicationBuilder<UpdateContext>
    {
        /// <summary>
        /// 更新上下文
        /// </summary>
        public UpdateContext Context { get; private set; }

        /// <summary>
        /// 应用程序创建者
        /// </summary>
        /// <param name="appServices"></param>
        private UpdateApp(UpdateContext context)
            : base(context.CreateServiceProvider())
        {
            this.Context = context;
        }
        public static UpdateApp CreateBuilder(UpdateContext context)
        {
            var builder = new UpdateApp(context); 
            builder.Use<ICheckUpdate>();
            builder.Use<IBackup>();
            builder.Use<IDownload>();
            builder.Use<IHashCheck>();
            builder.Use<IDecompress>();
            builder.Use<IUpgrad>();
            builder.Use<IStartUpgrad>();

            return builder;
        }

        ///// <summary>
        ///// 检查更新
        ///// </summary>
        //public async Task CheckForUpdateAsync()
        //{
        //    try
        //    {
        //        //var versionService = new VersionService();
        //        //var latestVersion = await versionService.GetLatestVersionAsync(Context.AppId);

        //        //if (latestVersion.Version > Context.CurrentVersion)
        //        //{
        //        //    OnFindNewVersion?.Invoke(Context);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        Context.OnUpdateException?.Invoke(ex);
        //    }
        //}

        /// <summary>
        /// 开始更新
        /// </summary>
        public async Task StartUpdateAsync()
        {
            try
            {
                await Build().Invoke(Context);
                //OnUpdateCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                Context.OnUpdateException?.Invoke(ex);
            }
        }

        ///// <summary>
        ///// 取消更新
        ///// </summary>
        //public void CancelUpdate()
        //{
        //    //Context.CancelToken.Cancel();
        //    //OnUpdateCanceled?.Invoke();
        //}
        /// <summary>
        /// 查找versions 中最大Version 且 RequiredMinVersion 小于 clientVersion
        /// </summary>
        /// <param name="versions"></param>
        /// <param name="clientVersion"></param>
        /// <returns></returns>
        public static VersionInfo? GetUpdateVersion(List<VersionInfo> versions, Version clientVersion)
        {
            var listVers = versions.OrderByDescending(x => x.Version).ToList();
            foreach (var verinfo in listVers)
            {
                if (verinfo.Version > clientVersion)
                {
                    if (!string.IsNullOrEmpty(verinfo.RequiredMinVersion) && (clientVersion < new Version(verinfo.RequiredMinVersion)))
                    {
                        continue;
                    }
                    return verinfo;
                }
                else
                {
                    return null;
                }
            }
            return null;

        }
    }
}
