using System.Collections.Generic;
using System.IO;
using Core.Interfaces;

namespace Repository.Files
{
    public class FileManager : IFileManager
    {
        private readonly ILogger? _logger;

        public FileManager(ILogger? logger = null)
        {
            _logger = logger;
        }
        public List<string> ReadLines(string path)
        {
            var lines = new List<string>();
            try
            {
                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                    return lines;
                }
                lines.AddRange(File.ReadAllLines(path));
            }
            catch (IOException ex)
            {
                Report($"Failed to read file '{path}'", ex);
            }
            return lines;
        }

        public void WriteLines(string path, List<string> lines)
        {
            try
            {
                File.WriteAllLines(path, lines);
            }
            catch (IOException ex)
            {
                Report($"Failed to write file '{path}'", ex);
            }
        }

        public void AppendLine(string path, string line)
        {
            try
            {
                File.AppendAllLines(path, new[] { line });
            }
            catch (IOException ex)
            {
                Report($"Failed to append to file '{path}'", ex);
            }
        }

        private void Report(string message, Exception ex)
        {
            if (_logger != null)
                _logger.LogError(message, ex);
            else
                Console.Error.WriteLine($"[FileManager] {message}: {ex.Message}");
        }
    }
}