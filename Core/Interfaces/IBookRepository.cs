using System.Collections.Generic;
using Core.Models;

namespace Core.Interfaces
{
    public interface IBookRepository
    {
        List<Book> GetAll();
        Book GetByIsbn(string isbn);
        List<Book> Search(string keyword);
        void Add(Book book);
        void Remove(string isbn);
        void Update(Book book);
    }
}