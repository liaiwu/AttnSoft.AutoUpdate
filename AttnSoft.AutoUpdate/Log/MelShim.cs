#if !HAS_MEL
using System;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// 与 Microsoft.Extensions.Logging.LogLevel 的命名空间、成员、数值完全一致的兼容定义。
    /// 本 shim 只编进零依赖的 Upgrade.exe；核心库各目标框架都直接引用
    /// Microsoft.Extensions.Logging.Abstractions，用的是真 MEL 类型。
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5,
        None = 6
    }

    /// <summary>
    /// 与 Microsoft.Extensions.Logging.EventId 一致的兼容定义。
    /// </summary>
    public readonly struct EventId : IEquatable<EventId>
    {
        public EventId(int id, string? name = null)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string? Name { get; }

        public bool Equals(EventId other) => Id == other.Id;

        public override bool Equals(object? obj) => obj is EventId other && Equals(other);

        public override int GetHashCode() => Id;

        public override string ToString() => Name ?? Id.ToString();

        public static implicit operator EventId(int id) => new EventId(id);

        public static bool operator ==(EventId left, EventId right) => left.Equals(right);

        public static bool operator !=(EventId left, EventId right) => !left.Equals(right);
    }

    /// <summary>
    /// 与 Microsoft.Extensions.Logging.ILogger 一致的兼容定义。
    /// </summary>
    public interface ILogger
    {
        void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter);

        bool IsEnabled(LogLevel logLevel);

        IDisposable? BeginScope<TState>(TState state) where TState : notnull;
    }
}
#endif
