using AttnSoft.AutoUpdate.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate
{
    /// <summary>
    /// 自动更新核心类
    /// </summary>
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
            builder.Use<ICheckCompletion>();
            builder.Use<ICheckUpdate>();
            builder.Use<IBackup>();
            builder.Use<IDownload>();
            builder.Use<IHashCheck>();
            builder.Use<IDecompress>();
            builder.Use<IUpgrad>();
            builder.Use<IStartUpgrad>();

            return builder;
        }

        /// <summary>
        /// 开始更新
        /// </summary>
        public async Task StartUpdateAsync()
        {
            try
            {
                // 交棒线程池，两个理由：
                // 1) async 方法在第一个 await 之前必然同步跑在调用方线程上（CLR 语义），
                //    不包这一层，管道开头那段就还在宿主线程上；
                // 2) net461 的下载器此前用 WebClient，其内部 AsyncOperation 会自己捕获上下文，
                //    在宿主 UI 线程上调用会把进度与完成投递回去（实测 Post 12 次）。
                //    现已换成 HttpWebRequest + 全程 ConfigureAwait(false)，不再投递（实测 Post 0 次），
                //    这一层现在只由上面理由 1 支撑。
                // 消费者的 await 仍回到他们自己的上下文，用法不变。
                await Task.Run(() => Build().Invoke(Context)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                UpdateLog.Error("升级流程失败", ex);
                Context.OnUpdateException?.Invoke(ex);
            }
        }
        /// <summary>
        /// 查找versions 中最大Version 且 RequiredMinVersion 小于 clientVersion
        /// </summary>
        /// <param name="versions"></param>
        /// <param name="clientVersion"></param>
        /// <returns></returns>
        public static VersionInfo? GetUpdateVersion(List<VersionInfo> versions, UpdateContext context)
        {
            Version clientVersion = context.ClientVersion;
            Version? skipVersion= context.LocalVerManager.GetVersionInfo()?.SkipVersion;

            var listVers = versions.OrderByDescending(x => x.Version).ToList();
            foreach (var verinfo in listVers)
            {
                if (verinfo.Version > clientVersion)
                {
                    if (!string.IsNullOrEmpty(verinfo.RequiredMinVersion) 
                        && (clientVersion < new Version(verinfo.RequiredMinVersion)))
                    {
                        continue;
                    }
                    if ((skipVersion != null && skipVersion == verinfo.Version))
                    {
                        continue;
                    }
                    return verinfo;
                }
                else
                {
                    break;
                }
            }
            return null;

        }
    }
}
