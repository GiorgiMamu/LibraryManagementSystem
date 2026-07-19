using System.Collections.Generic;
using Core.Models;

namespace Core.Interfaces
{
    // contract for anything that stores/retrieves Users
    // repository will implement this using text files
    // services will only ever call through this interface
    public interface IUserRepository
    {
        List<User> GetAll();
        User? GetById(int id);
        User? GetByUsername(string username);
        User? GetByEmail(string email);
        void Add(User user);
        void Update(User user); 
    }
}