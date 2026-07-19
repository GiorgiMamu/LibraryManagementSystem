using System;
using Core.Enums;
using Core.Interfaces;
using Core.Models;

namespace Repository.Files
{
    public class FileLogger : ILogger
    {
        private readonly IFileManager _fileManager;
        private readonly string _logPath;

        public FileLogger(IFileManager fileManager, string logPath = @"C:\Users\oto\Desktop\LibraryManagementSystem\Repository\Data\logs.txt")
        {
            _fileManager = fileManager;
            _logPath = logPath;
        }

        public void LogInfo(string message) => Write(LogLevel.Info, message);
        public void LogWarning(string message) => Write(LogLevel.Warning, message);

        public void LogError(string message, Exception? ex = null)
        {
            var full = ex == null ? message : $"{message} :: {ex.Message}";
            Write(LogLevel.Error, full);
        }

        private void Write(LogLevel level, string message)
        {
            var entry = new LogEntry(level, message);
            _fileManager.AppendLine(_logPath, entry.ToString());
        }
    }
}