using System.Linq;
using Core.Enums;
using Core.Exceptions;
using Core.Helpers;
using Core.Interfaces;
using Core.Models;

namespace Services
{
    public class AuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        // both dependencies are interfaces, not concrete classes —
        // this class never knows it's a text file or that it's BCrypt underneath.
        public AuthenticationService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public User Register(string username, string password, UserRole role)
        {
            if (!Validator.IsNotEmpty(username) || !Validator.IsNotEmpty(password))
                throw new System.ArgumentException("Username and password cannot be empty.");

            if (_userRepository.GetByUsername(username) != null)
                throw new DuplicateUserException(username);

            int nextId = ComputeNextId();
            string hashedPassword = _passwordHasher.Hash(password);

            User newUser = role == UserRole.Admin
                ? new AdminUser(nextId, username, hashedPassword)
                : new ClientUser(nextId, username, hashedPassword);

            _userRepository.Add(newUser);
            return newUser;
        }

        public User Login(string username, string password)
        {
            var user = _userRepository.GetByUsername(username);

            // same error either way (user not found OR wrong password) (so we don't give away which one it was, hehe)
            if (user == null || !_passwordHasher.Verify(password, user.PasswordHash))
                throw new InvalidCredentialsException();

            return user;
        }

       // lets use some LINQ
        private int ComputeNextId()
        {
            var all = _userRepository.GetAll();
            return all.Count == 0 ? 1 : all.Max(u => u.Id) + 1;
        }
    }
}