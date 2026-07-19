using System;
using System.Linq;
using Core.Enums;
using Core.Exceptions;
using Core.Helpers;
using Core.Interfaces;
using Core.Models;
using Services.Interfaces;

namespace Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;

        public AuthenticationService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IEmailService emailService,
            ILogger logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _logger = logger;
        }

        public User Register(string username, string email, string password, UserRole role)
        {
            if (!Validator.IsValidUsername(username))
                throw new ArgumentException("Username must start with a letter and contain only letters, digits, or underscores");

            if (!Validator.IsStrongPassword(password))
                throw new ArgumentException("Password must be at least 8 characters and include an uppercase letter, a lowercase letter, a number, and a symbol");

            if (!Validator.IsValidEmail(email))
                throw new ArgumentException("Email address is not valid");

            if (_userRepository.GetByUsername(username) != null)
                throw new DuplicateUserException(username);

            if (_userRepository.GetByEmail(email) != null)
                throw new DuplicateUserException(email);

            int nextId = ComputeNextId();
            string hashedPassword = _passwordHasher.Hash(password);
            string verificationCode = GenerateVerificationCode();

            User newUser = role == UserRole.Admin
                ? new AdminUser(nextId, username, email, hashedPassword, verificationCode)
                : new ClientUser(nextId, username, email, hashedPassword, verificationCode);

            _userRepository.Add(newUser);

            _emailService.SendEmail(
                email,
                "Verify your Library account",
                $"Hello {username},\n\nYour verification code is: {verificationCode}");

            _logger.LogInfo($"User '{username}' registered, verification email sent to {email}.");
            return newUser;
        }


        public void VerifyAccount(string email, string code)
        {
            if (!Validator.IsValidEmail(email))
                throw new ArgumentException("Email address is not valid.");
            if (!Validator.IsNotEmpty(code))
                throw new ArgumentException("Verification code cannot be empty.");

            // ?? If the user is not found, throw a UserNotFoundException with the email (if-null)
            var user = _userRepository.GetByEmail(email)
                ?? throw new UserNotFoundException(email);

            if (user.IsVerified) return;

            if (!user.VerifyAccount(code))
                throw new InvalidVerificationCodeException();

            _userRepository.Update(user);
            _logger.LogInfo($"User '{user.Username}' verified their account.");
        }

        //generates and sends a brand-new code, replacing the old one
        public void ResendVerificationCode(string email)
        {
            if (!Validator.IsValidEmail(email))
                throw new ArgumentException("Email address is not valid.");

            var user = _userRepository.GetByEmail(email)
                ?? throw new UserNotFoundException(email);

            if (user.IsVerified) return;

            string newCode = GenerateVerificationCode();
            user.ResetVerificationCode(newCode);
            _userRepository.Update(user);

            _emailService.SendEmail(email, "Your new verification code",
                $"Your new verification code is: {newCode}");

            _logger.LogInfo($"Resent verification code to {email}.");
        }

        public User Login(string username, string password)
        {
            if (!Validator.IsNotEmpty(username) || !Validator.IsNotEmpty(password))
                throw new InvalidCredentialsException();

            var user = _userRepository.GetByUsername(username);

            if (user == null || !_passwordHasher.Verify(password, user.PasswordHash))
                throw new InvalidCredentialsException();

            if (!user.IsVerified)
                throw new AccountNotVerifiedException(username);

            return user;
        }

        private int ComputeNextId()
        {
            var all = _userRepository.GetAll();
            return all.Count == 0 ? 1 : all.Max(u => u.Id) + 1;
        }

        private static string GenerateVerificationCode() =>
            new Random().Next(1000, 9999).ToString();

        public User? FindByEmail(string email) => _userRepository.GetByEmail(email);
        public User? GetUserById(int id) => _userRepository.GetById(id);


    }
}