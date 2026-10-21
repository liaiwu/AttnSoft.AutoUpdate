using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AttnSoft.AutoUpdate
{
    /// <summary>
    /// 追加写入单个日志文件的输出目标。每条记录立即落盘：
    /// 升级器安装完成后以 Kill() 结束自身，缓冲区里未刷出的记录会随之丢失。
    /// 写日志本身的失败一律吞掉，不能反过来中断升级。
    /// </summary>
    public sealed class FileUpdateLogger : ILogger
    {
        private readonly string _path;
        private readonly object _gate = new object();

        /// <summary>
        /// 指定日志文件路径，必要时创建其所在目录。目录创建失败会抛出，由调用方决定回退。
        /// </summary>
        public FileUpdateLogger(string path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var text = formatter(state, exception);
            if (exception is not null) text += $" | {exception}";
            try
            {
                lock (_gate)
                {
                    // share 必须放宽到 ReadWrite：Upgrade.exe 与宿主进程会并发写同一个 upgrade.log，而 File.AppendAllText 用的是 FileShare.Read
                    using var fs = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    // 用三参重载：Stream.Write(byte[]) 是 .NET Core 2.1+ 才有的，net461 没有
                    byte[] bytes = Encoding.UTF8.GetBytes(
                        $"{UpdateLog.CategoryPrefix}[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel}] {text}{Environment.NewLine}");
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            catch
            {
                // 日志不可用不应影响升级流程
            }
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
            => logLevel is not LogLevel.None && logLevel >= UpdateLog.MinimumLevel;

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }
}
