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

            bool emailSent = _emailService.SendEmail(
                email,
                "Verify your Library Account",
                $@"
    <div style='max-width:500px;margin:40px auto;padding:30px;
                font-family:Arial,sans-serif;
                border:1px solid #e5e5e5;
                border-radius:10px;
                background:#ffffff;
                text-align:center;'>

        <h2 style='color:#2563eb;margin-bottom:10px;'>
            📚 Library System
        </h2>

        <p style='font-size:16px;color:#444;'>
            Hello <strong>{username}</strong>,
        </p>

        <p style='color:#666;'>
            Thank you for registering! Use the verification code below to activate your account.
        </p>

        <div style='font-size:30px;
                    font-weight:bold;
                    letter-spacing:5px;
                    color:#2563eb;
                    background:#f3f7ff;
                    padding:15px;
                    margin:25px 0;
                    border-radius:8px;'>
            {verificationCode}
        </div>

        <p style='font-size:13px;color:#888;'>
            If you didn't create this account, simply ignore this email.
        </p>

    </div>");

            if (!emailSent)
            {
                _logger.LogWarning($"User '{username}' registered, but verification email to {email} failed to send.");
                throw new EmailSendException(email);
            }

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

            bool emailSent = _emailService.SendEmail(
    email,
    "Your New Verification Code",
    $@"
    <div style='max-width:500px;margin:40px auto;padding:30px;
                font-family:Arial,sans-serif;
                border:1px solid #e5e5e5;
                border-radius:10px;
                background:#ffffff;
                text-align:center;'>

        <h2 style='color:#2563eb;margin-bottom:10px;'>
            📚 Library System
        </h2>

        <p style='font-size:16px;color:#444;'>
            Here is your new verification code:
        </p>

        <div style='font-size:30px;
                    font-weight:bold;
                    letter-spacing:5px;
                    color:#2563eb;
                    background:#f3f7ff;
                    padding:15px;
                    margin:25px 0;
                    border-radius:8px;'>
            {newCode}
        </div>

        <p style='font-size:13px;color:#888;'>
            If you didn't request a new verification code, you can safely ignore this email.
        </p>

    </div>");

            if (!emailSent)
            {
                _logger.LogWarning($"New verification code generated for {email}, but the email failed to send.");
                throw new EmailSendException(email);
            }

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


        // self-service password change — requires the current password,
        // used by the Settings menu for both Admin and Client
        public void ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = _userRepository.GetById(userId)
                ?? throw new UserNotFoundException(userId.ToString());

            if (!_passwordHasher.Verify(currentPassword, user.PasswordHash))
                throw new InvalidCredentialsException();

            if (!Validator.IsStrongPassword(newPassword))
                throw new ArgumentException("Password must be at least 8 characters and include an uppercase letter, a lowercase letter, a number, and a symbol");

            user.ChangePassword(_passwordHasher.Hash(newPassword));
            _userRepository.Update(user);
            _logger.LogInfo($"User '{user.Username}' changed their password.");

            _emailService.SendEmail(
    user.Email,
    "Your Password Was Changed",
    $@"
    <div style='max-width:500px;margin:40px auto;padding:30px;
                font-family:Arial,sans-serif;
                border:1px solid #e5e5e5;
                border-radius:10px;
                background:#ffffff;
                text-align:center;'>

        <h2 style='color:#2563eb;margin-bottom:10px;'>
            📚 Library System
        </h2>

        <p style='font-size:16px;color:#444;'>
            Hello <strong>{user.Username}</strong>,
        </p>

        <p style='color:#666;'>
            Your account password has been changed successfully.
        </p>

        <p style='color:#666;'>
            If you made this change, no further action is required.
        </p>

        <p style='color:#d32f2f;font-weight:bold;'>
            If you didn't change your password, please contact an administrator immediately.
        </p>

    </div>");
        }

        // self-service account deletion — requires the current password
        public void DeleteAccount(int userId, string currentPassword)
        {
            var user = _userRepository.GetById(userId)
                ?? throw new UserNotFoundException(userId.ToString());

            if (!_passwordHasher.Verify(currentPassword, user.PasswordHash))
                throw new InvalidCredentialsException();

            _userRepository.Remove(userId);
            _logger.LogInfo($"User '{user.Username}' (ID {userId}) deleted their own account.");
        }

        // admin-only: list every user in the system
        public List<User> GetAllUsers() => _userRepository.GetAll();

        // admin-only: delete any user without needing that user's password
        public void DeleteUser(int userId)
        {
            var user = _userRepository.GetById(userId)
                ?? throw new UserNotFoundException(userId.ToString());

            _userRepository.Remove(userId);
            _logger.LogInfo($"Admin deleted user '{user.Username}' (ID {userId}).");
        }

    }
}