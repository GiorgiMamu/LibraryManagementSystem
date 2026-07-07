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
            _quantity = quantity < 0 ? 0 : quantity;
        }

        public string Isbn => _isbn;
        public string Title => _title;
        public string Author => _author;
        public int Quantity => _quantity;
        public bool IsAvailable => _quantity > 0;

        public void Decrease(int amount = 1)
        {
            if (_quantity - amount < 0)
                throw new InsufficientQuantityException(_isbn);
            _quantity -= amount;
        }

        public void Increase(int amount = 1)
        {
            if (amount > 0) _quantity += amount;
        }
    }
}