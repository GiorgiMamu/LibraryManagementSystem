using Core.Enums;
using System.Collections.Generic;


namespace Core.Models
{
    public class ClientUser : User
    {
        // fines only for clients, so we store it here. Admins don't have fines
        private decimal _fines;

        public ClientUser(int id, string username, string email, string passwordHash,
            string verificationCode, bool isVerified = false, decimal fines = 0m)
            : base(id, username, email, passwordHash, verificationCode, isVerified)
        {
            _fines = fines;
        }


        // set the role to Client for this subclass
        public override UserRole Role => UserRole.Client;

        public decimal Fines => _fines;


        // client with any unpaid fine can't borrow
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
       

    }
}