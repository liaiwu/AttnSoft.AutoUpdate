#if !NETFRAMEWORK
using Microsoft.Extensions.Logging;

namespace AttnSoft.AutoUpdate
{
    /// <summary>
    /// 日志接入扩展。
    /// </summary>
    public static class UpdateLogExtensions
    {
        /// <summary>
        /// 把升级日志接入宿主的 Microsoft.Extensions.Logging 管道（Serilog、NLog、Console 等均可），
        /// 级别、格式与落盘策略全部交由宿主的日志配置决定。
        /// 为此会把 <see cref="UpdateLog.MinimumLevel"/> 放宽到 Trace，否则本库会先把宿主要看的记录挡掉。
        /// 这是进程级设置，应在应用启动时调用一次。
        /// <para>
        /// <see cref="UpdateLog.Category"/> 非空时作为 MEL 的 category 名，宿主即可按来源过滤与路由
        /// （如 <c>Logging:LogLevel:MyApp</c>，或 Serilog 的 SourceContext 过滤）。MEL 的 category 在调用
        /// 本方法时即固定，要让它生效必须先设 <see cref="UpdateLog.Category"/> 再调用。
        /// </para>
        /// </summary>
        public static UpdateContext UseMicrosoftLogging(this UpdateContext context, ILoggerFactory loggerFactory)
        {
            // Category 交给 MEL 当 category 用，宿主才能按来源做级别过滤与路由；拼进消息文本就失去这个能力。
            var categoryName = string.IsNullOrEmpty(UpdateLog.Category) ? "AttnSoft.AutoUpdate" : UpdateLog.Category;
            UpdateLog.Sink = loggerFactory.CreateLogger(categoryName);
            UpdateLog.MinimumLevel = LogLevel.Trace;
            return context;
        }
    }
}
#endif
