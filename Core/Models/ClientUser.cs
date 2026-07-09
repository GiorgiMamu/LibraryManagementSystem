using Core.Enums;

namespace Core.Models
{
    public class ClientUser : User
    {
        // fines only for clients, so we store it here. Admins don't have fines.
        private decimal _fines;

        public ClientUser(int id, string username, string passwordHash, decimal fines = 0m)
            : base(id, username, passwordHash)
        {
            _fines = fines;
        }


        // set the role to Client for this subclass
        public override UserRole Role => UserRole.Client;

        public decimal Fines => _fines;


        // client with any unpaid fine can't borrow.
        public bool CanBorrow() => Fines == 0m;

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

        public override void DisplayMenu()
        {
            System.Console.WriteLine("--- Client menu ---");
            System.Console.WriteLine("1. Browse catalogue");
            System.Console.WriteLine("2. Search books");
            System.Console.WriteLine("3. Borrow a book");
            System.Console.WriteLine("4. Return a book");
            System.Console.WriteLine("5. View my fines");
            System.Console.WriteLine("0. Logout");
        }
    }
}