using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AttnSoft.AutoUpdate
{
    // 实现一个 ILogger 工厂
    public class LoggerFactory
    {
        public static ILogger<T> GetLogger<T>()
        {
            return new ConsoleLogger<T>();
        }
    }
    public class ConsoleLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (IsEnabled(logLevel))
            {
                Console.WriteLine($"[{typeof(T).Name}] {formatter(state, exception)}");
            }
        }
    }
}
