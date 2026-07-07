using Core.Enums;

namespace Core.Models
{
    public class ClientUser : User
    {
        public ClientUser(int id, string username, string passwordHash, decimal fines = 0m)
            : base(id, username, passwordHash, fines) { }

        public override UserRole Role => UserRole.Client;

        public bool CanBorrow() => Fines == 0m;

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