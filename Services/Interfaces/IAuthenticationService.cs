using Core.Enums;
using Core.Models;

namespace Services.Interfaces
{
    public interface IAuthenticationService
    {
        User Register(string username, string email, string password, UserRole role);
        void VerifyAccount(string username, string code);
        void ResendVerificationCode(string email);

        User Login(string username, string password);
        User? FindByEmail(string email); 
        User? GetUserById(int id); 

    }
}