using Core.Enums;
using System.Collections.Generic;


namespace Core.Models
{
    public class AdminUser : User
    {
        public AdminUser(int id, string username, string email, string passwordHash,
            string verificationCode, bool isVerified = false)
            : base(id, username, email, passwordHash, verificationCode, isVerified) { }


        // set the role to Admin for this subclass
        public override UserRole Role => UserRole.Admin;

        
    }

}