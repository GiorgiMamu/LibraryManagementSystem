using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Core.Models;

namespace Repository
{
    // File format:
    // ID | Username | Email | PasswordHash | Role | Fines | IsVerified | VerificationCode
    public class UserRepository : IUserRepository
    {
        private readonly IFileManager _fileManager;
        private readonly ILogger _logger;
        private readonly string _path;

        public UserRepository(IFileManager fileManager, ILogger logger, string path = @"C:\Users\oto\Desktop\LibraryManagementSystem\Repository\Data\users.txt")
        {
            _fileManager = fileManager;
            _logger = logger;
            _path = path;
        }

        public List<User> GetAll()
        {
            var users = new List<User>();
            foreach (var line in _fileManager.ReadLines(_path))
            {
                var user = ParseLine(line);
                if (user != null) users.Add(user);
            }
            return users;
        }

        public User? GetById(int id) => GetAll().FirstOrDefault(u => u.Id == id);

        public User? GetByUsername(string username) =>
            GetAll().FirstOrDefault(u => u.Username == username);

        public User? GetByEmail(string email) =>
            GetAll().FirstOrDefault(u => u.Email == email);
        public void Add(User user) => _fileManager.AppendLine(_path, ToLine(user));

        public void Update(User user)
        {
            var lines = GetAll()
                .Select(u => u.Id == user.Id ? ToLine(user) : ToLine(u))
                .ToList();
            _fileManager.WriteLines(_path, lines);
        }

        public void Remove(int id)
        {
            var remaining = GetAll().Where(u => u.Id != id).Select(ToLine).ToList();
            _fileManager.WriteLines(_path, remaining);
        }

        private User? ParseLine(string line)
        {
            try
            {
                var parts = line.Split('|').Select(p => p.Trim()).ToArray();
                if (parts.Length < 8) return null;

                int id = int.Parse(parts[0]);
                string username = parts[1];
                string email = parts[2];
                string passwordHash = parts[3];
                string role = parts[4].ToLowerInvariant();
                decimal fines = decimal.Parse(parts[5]);
                bool isVerified = bool.Parse(parts[6]);
                string verificationCode = parts[7];

                return role == "admin"
                    ? new AdminUser(id, username, email, passwordHash, verificationCode, isVerified)
                    : new ClientUser(id, username, email, passwordHash, verificationCode, isVerified, fines);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Skipped malformed line in {_path}: '{line}'", ex);
                return null;
            }
        }

        private static string ToLine(User u)
        {
            // if u is ClientUser, convert to client, otherwise fines is 0
            decimal fines = (u is ClientUser client) ? client.Fines : 0m;
            return $"{u.Id} | {u.Username} | {u.Email} | {u.PasswordHash} | " +
                   $"{u.Role.ToString().ToLowerInvariant()} | {fines:F2} | {u.IsVerified} | {u.VerificationCode}";
        }
    }
}