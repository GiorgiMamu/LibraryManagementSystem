using System.Collections.Generic;
using Core.Models;

namespace Core.Interfaces
{
    public interface IUserRepository
    {
        List<User> GetAll();
        User GetById(int id);
        User GetByUsername(string username);
        void Add(User user);
        void Update(User user);
    }
}