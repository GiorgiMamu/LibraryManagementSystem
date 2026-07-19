using Core.Enums;
using System.Collections.Generic;

namespace Core.Models
{
    // abstract base class for all users in the system
    public abstract class User
    {
        // private backing fields: nothing outside this class can touch these directly
        private readonly int _id;
        private readonly string _username;
        private readonly string _email;
        private readonly string _passwordHash;
        private string _verificationCode; // can be reset on resend
        private bool _isVerified;

        protected User(int id, string username, string email, string passwordHash,
           string verificationCode, bool isVerified = false)
        {
            _id = id;
            _username = username;
            _email = email;
            _passwordHash = passwordHash;
            _verificationCode = verificationCode;
            _isVerified = isVerified;
        }
        //  outside code can read these, never set them directly
        public int Id => _id;
        public string Username => _username;
        public string Email => _email;
        public string PasswordHash => _passwordHash;
        public string VerificationCode => _verificationCode;
        public bool IsVerified => _isVerified;


        // each subclass must say what role it is — this is what lets
        // UserRepository know whether to build a ClientUser or AdminUser
        // when reading a line back from the file
        public abstract UserRole Role { get; }


        // the only way IsVerified can flip to true
        public bool VerifyAccount(string code)
        {
            if (_isVerified) return true;
            if (_verificationCode != code) return false;
            _isVerified = true;
            return true;
        }

        // The only way the code can change after registration — used when
        // resending a fresh code after a wrong attempt
        public void ResetVerificationCode(string newCode)
        {
            if (_isVerified) return;
            _verificationCode = newCode;
        }


    }
}