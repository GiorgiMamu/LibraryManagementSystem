using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Core.Models;

namespace Repository
{
    // File format: ISBN | Title | Author | Quantity
    public class BookRepository : IBookRepository
    {
        private readonly IFileManager _fileManager;
        private readonly ILogger _logger;
        private readonly string _path;

        public BookRepository(IFileManager fileManager, ILogger logger, string path = @"C:\Users\oto\Desktop\LibraryManagementSystem\Repository\Data\books.txt")
        {
            _fileManager = fileManager;
            _logger = logger;
            _path = path;
        }

        public List<Book> GetAll()
        {
            var books = new List<Book>();
            foreach (var line in _fileManager.ReadLines(_path))
            {
                var book = ParseLine(line);
                if (book != null) books.Add(book);
            }
            return books;
        }

        public Book? GetByIsbn(string isbn) =>
            GetAll().FirstOrDefault(b => b.Isbn == isbn);

        public List<Book> Search(string keyword) =>
            GetAll().Where(b =>
                b.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

        public void Add(Book book) => _fileManager.AppendLine(_path, ToLine(book));

        public void Remove(string isbn)
        {
            var remaining = GetAll().Where(b => b.Isbn != isbn).Select(ToLine).ToList();
            _fileManager.WriteLines(_path, remaining);
        }

        public void Update(Book book)
        {
            var lines = GetAll()
                .Select(b => b.Isbn == book.Isbn ? ToLine(book) : ToLine(b))
                .ToList();
            _fileManager.WriteLines(_path, lines);
        }

        private Book? ParseLine(string line)
        {
            try
            {
                var parts = line.Split('|').Select(p => p.Trim()).ToArray();
                if (parts.Length < 4) return null;
                return new Book(parts[0], parts[1], parts[2], int.Parse(parts[3]));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Skipped malformed line in {_path}: '{line}'", ex);
                return null;
            }
        }

        private static string ToLine(Book b) =>
            $"{b.Isbn} | {b.Title} | {b.Author} | {b.Quantity}";
    }
}