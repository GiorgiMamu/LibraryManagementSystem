using Core.Enums;

namespace Core.Models
{
    public abstract class User
    {
        private readonly int _id;
        private readonly string _username;
        private readonly string _passwordHash;
        private decimal _fines;

        protected User(int id, string username, string passwordHash, decimal fines = 0m)
        {
            _id = id;
            _username = username;
            _passwordHash = passwordHash;
            _fines = fines;
        }

        public int Id => _id;
        public string Username => _username;
        public decimal Fines => _fines;
        internal string PasswordHash => _passwordHash;

        public abstract UserRole Role { get; }

        public void AddFine(decimal amount)
        {
            if (amount <= 0) return;
            _fines += amount;
        }

        public void PayFine(decimal amount)
        {
            if (amount <= 0) return;
            _fines = amount >= _fines ? 0m : _fines - amount;
        }

        public bool VerifyPassword(string plainPassword) =>
            _passwordHash == plainPassword;

        public abstract void DisplayMenu();
    }
}