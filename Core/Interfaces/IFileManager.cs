using System.Collections.Generic;

namespace Core.Interfaces
{
    // generic contract for reading/writing plain text files
    // any repository (User, Book, Borrow) uses this instead of
    // calling File.ReadAllLines directly — keeps file-handling code
    // in exactly one place (FileManager, in the Repository project)
    public interface IFileManager
    {
        List<string> ReadLines(string path);
        void WriteLines(string path, List<string> lines);
        void AppendLine(string path, string line);
    }
}