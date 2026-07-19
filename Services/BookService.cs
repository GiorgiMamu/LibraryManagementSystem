using System.Collections.Generic;
using Core.Exceptions;
using Core.Interfaces;
using Core.Models;
using Services.Interfaces;

namespace Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger _logger;

        public BookService(IBookRepository bookRepository, ILogger logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public List<Book> GetAll() => _bookRepository.GetAll();
        public List<Book> Search(string keyword) => _bookRepository.Search(keyword);

        public void AddBook(string isbn, string title, string author, int quantity)
        {
            _bookRepository.Add(new Book(isbn, title, author, quantity));
            _logger.LogInfo($"Book added: {isbn} - {title}");
        }

        public void RemoveBook(string isbn)
        {
            var book = _bookRepository.GetByIsbn(isbn) ?? throw new BookNotFoundException(isbn);
            _bookRepository.Remove(book.Isbn);
            _logger.LogInfo($"Book removed: {isbn}");
        }

        public void AdjustQuantity(string isbn, int delta)
        {
            var book = _bookRepository.GetByIsbn(isbn) ?? throw new BookNotFoundException(isbn);

            if (delta > 0) book.Increase(delta);
            else book.Decrease(-delta);

            _bookRepository.Update(book);
            _logger.LogInfo($"Quantity adjusted for {isbn}: {delta:+#;-#;0}");
        }
    }
}