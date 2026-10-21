using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AttnSoft.AutoUpdate
{
    /// <summary>
    /// 默认日志输出目标，写入 <see cref="Trace"/>。
    /// 不重定向 Console.Out、不写文件；仅会把 <see cref="Trace.AutoFlush"/> 置为 true，
    /// 否则进程退出时缓冲区里的日志会丢。消费者可用 Trace.Listeners.Add(...) 自行决定日志去向。
    /// </summary>
    public sealed class TraceUpdateLogger : ILogger
    {
        /// <summary>
        /// 共享实例。
        /// </summary>
        public static readonly TraceUpdateLogger Instance = new TraceUpdateLogger();

        static TraceUpdateLogger()
        {
            // 默认 Trace 会缓冲。升级失败时进程往往直接退出，
            // 缓冲区里的日志跟着丢失，而崩溃时刻正是最需要日志的时刻。
            Trace.AutoFlush = true;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var text = formatter(state, exception);
            if (exception is not null) text += $" | {exception}";
            Trace.WriteLine($"{UpdateLog.CategoryPrefix}[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel}] {text}");
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
            => logLevel is not LogLevel.None && logLevel >= UpdateLog.MinimumLevel;

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }
}
