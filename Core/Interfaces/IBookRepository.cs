using System.Collections.Generic;
using Core.Models;

namespace Core.Interfaces
{
    // contract for anything that stores/retrieves Books
    // repository will implement this using text files
    // services will only ever call through this interface
    public interface IBookRepository
    {
        List<Book> GetAll();
        Book? GetByIsbn(string isbn);
        List<Book> Search(string keyword); // title/author search feature
        void Add(Book book);
        void Remove(string isbn);
        void Update(Book book); // used after quantity changes
    }
}