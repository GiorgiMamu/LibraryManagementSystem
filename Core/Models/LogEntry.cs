using System;
using Core.Enums;

namespace Core.Models
{
    // Represents a log entry in the system
    public class LogEntry
    {
        public DateTime Timestamp { get; }
        public LogLevel Level { get; }
        public string Message { get; }

        public LogEntry(LogLevel level, string message)
        {
            Timestamp = DateTime.Now;
            Level = level;
            Message = message;
        }

        public override string ToString() =>
            $"{Timestamp:yyyy-MM-dd HH:mm:ss} | {Level} | {Message}";
    }
}