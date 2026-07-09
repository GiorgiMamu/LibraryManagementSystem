using Core.Exceptions;

namespace Core.Models
{
    public class Book
    {
        private readonly string _isbn;
        private readonly string _title;
        private readonly string _author;
        private int _quantity;

        public Book(string isbn, string title, string author, int quantity)
        {
            _isbn = isbn;
            _title = title;
            _author = author;
            // extra defense
            _quantity = quantity < 0 ? 0 : quantity;
        }

        // properties to expose the private fields (no setting)
        public string Isbn => _isbn;
        public string Title => _title;
        public string Author => _author;
        public int Quantity => _quantity;

        // convenience property to check if the book is available for borrowing
        public bool IsAvailable => _quantity > 0;


        // called when a borrow is approved. throws if it would go negative
        public void Decrease(int amount = 1)
        {
            if (_quantity - amount < 0)
                throw new InsufficientQuantityException(_isbn);
            _quantity -= amount;
        }

        // called when a book is returned, or admin restocks
        public void Increase(int amount = 1)
        {
            if (amount > 0) _quantity += amount;
        }
    }
}