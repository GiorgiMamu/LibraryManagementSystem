using System.Collections.Generic;
using Core.Models;

namespace Services.Interfaces
{
    public interface IBookService
    {
        List<Book> GetAll();
        List<Book> Search(string keyword);
        void AddBook(string isbn, string title, string author, int quantity);
        void RemoveBook(string isbn);
        void AdjustQuantity(string isbn, int delta);
    }
}