using Core.Enums;

namespace Core.Models
{
    public class AdminUser : User
    {
        public AdminUser(int id, string username, string passwordHash, decimal fines = 0m)
            : base(id, username, passwordHash, fines) { }

        public override UserRole Role => UserRole.Admin;

        public override void DisplayMenu()
        {
            System.Console.WriteLine("--- Admin Menu ---");
            System.Console.WriteLine("1. View all books");
            System.Console.WriteLine("2. Add a book");
            System.Console.WriteLine("3. Remove a book");
            System.Console.WriteLine("4. Adjust book quantity");
            System.Console.WriteLine("5. View pending borrow requests");
            System.Console.WriteLine("6. Approve / reject a request");
            System.Console.WriteLine("7. Send due-date notifications");
            System.Console.WriteLine("0. Logout");
        }
    }
}