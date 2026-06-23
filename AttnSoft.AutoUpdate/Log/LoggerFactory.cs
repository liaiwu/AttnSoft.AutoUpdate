using System;
using System.IO;
using System.Text;

namespace AttnSoft.AutoUpdate
{
    /// <summary>
    /// 将 Console.WriteLine 重定向到日志文件（带时间戳）。
    /// 调用 Initialize() 后，所有 Console.WriteLine 同时输出到文件和原始控制台。
    /// </summary>
    public static class LoggerFactory
    {
        private static readonly string _logPath;
        private static TextWriter? _originalOut;

        static LoggerFactory()
        {
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "upgrade_logs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            var dateStr = DateTime.Now.ToString("yyyyMMdd");
            _logPath = Path.Combine(logDir, $"upgrade_{dateStr}.log");
        }

        /// <summary>
        /// 获取当前日志文件路径。
        /// </summary>
        public static string LogPath => _logPath;

        /// <summary>
        /// 将 Console.Out 重定向到日志文件（带时间戳）。调用后 Console.WriteLine 同时输出到文件和原始控制台。
        /// </summary>
        public static void Initialize()
        {
            if (_originalOut != null) return;

            _originalOut = Console.Out;
            Console.SetOut(new TimestampWriter(
                new StreamWriter(_logPath, append: true, Encoding.UTF8, bufferSize: 1024),
                _originalOut));
        }

        /// <summary>
        /// 恢复 Console.Out 到原始输出。
        /// </summary>
        public static void Restore()
        {
            if (_originalOut != null)
            {
                Console.SetOut(_originalOut);
                _originalOut = null;
            }
        }
    }
}
