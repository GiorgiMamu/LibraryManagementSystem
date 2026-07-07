using System.Collections.Generic;

namespace Core.Interfaces
{
    public interface IFileManager
    {
        List<string> ReadLines(string path);
        void WriteLines(string path, List<string> lines);
        void AppendLine(string path, string line);
    }
}