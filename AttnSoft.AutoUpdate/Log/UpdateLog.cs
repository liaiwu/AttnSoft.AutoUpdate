using System;
using Microsoft.Extensions.Logging;

namespace AttnSoft.AutoUpdate
{
    /// <summary>
    /// 日志静态门面，本库所有日志都经由此处写出。
    /// <see cref="Sink"/> 与 <see cref="MinimumLevel"/> 应在应用启动时设置一次，运行期间不要并发修改。
    /// </summary>
    public static class UpdateLog
    {
        private static ILogger? _sink;

        /// <summary>
        /// 日志输出目标，默认写入 <see cref="Trace"/>（Release 下亦可见）。
        /// 设为自定义实现即可接管日志去向，例如落盘或转发给宿主的日志框架。
        /// 默认实例在首次取用时才创建，好让自带 Sink 的消费者完全不被本库对 <see cref="Trace"/> 的全局设置波及。
        /// </summary>
        public static ILogger Sink
        {
            get => _sink ??= TraceUpdateLogger.Instance;
            set => _sink = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 最低记录级别，默认 <see cref="LogLevel.Information"/>；设为 <see cref="LogLevel.None"/> 则完全静默。
        /// 低于该级别的日志不会交给 <see cref="Sink"/>，因此外部 Trace listener 也收不到。
        /// </summary>
        public static LogLevel MinimumLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// 日志来源标识，非空时每条记录以 <c>[Category]</c> 开头。
        /// 用于多个进程的输出汇到同一处时区分来源——例如 DebugView 会把所有进程的
        /// Trace 混在一起显示。默认不带前缀。
        /// </summary>
        public static string? Category { get; set; }

        internal static string CategoryPrefix => string.IsNullOrEmpty(Category) ? string.Empty : $"[{Category}] ";

        /// <summary>记录 Trace 级日志。</summary>
        public static void Trace(string message, Exception? exception = null)
            => Write(LogLevel.Trace, message, exception);

        /// <summary>记录 Debug 级日志。</summary>
        public static void Debug(string message, Exception? exception = null)
            => Write(LogLevel.Debug, message, exception);

        /// <summary>记录升级流程里程碑。</summary>
        public static void Information(string message, Exception? exception = null)
            => Write(LogLevel.Information, message, exception);

        /// <summary>记录可疑但不影响继续执行的情况。</summary>
        public static void Warning(string message, Exception? exception = null)
            => Write(LogLevel.Warning, message, exception);

        /// <summary>记录导致当前步骤失败的异常。</summary>
        public static void Error(string message, Exception? exception = null)
            => Write(LogLevel.Error, message, exception);

        /// <summary>记录导致升级无法继续的致命失败。</summary>
        public static void Critical(string message, Exception? exception = null)
            => Write(LogLevel.Critical, message, exception);

        private static void Write(LogLevel level, string message, Exception? exception)
        {
            if (level == LogLevel.None || level < MinimumLevel) return;
            try
            {
                Sink.Log(level, default, message, exception, static (state, _) => state);
            }
            catch (Exception ex)
            {
                // sink 由消费者提供，它失败不能反过来中断升级——本方法还常在 catch 块内被调用。
                // 兜底走 Trace 而不是走 Sink：sink 本身就是故障点时不会再递归回去。
                System.Diagnostics.Trace.WriteLine($"{CategoryPrefix}日志输出失败（{level}）：{ex.Message}");
            }
        }
    }
}
