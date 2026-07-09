using Core.Enums;

namespace Core.Models
{
    // abstract base class for all users in the system
    public abstract class User
    {
        // private backing fields: nothing outside this class can touch these directly
        private readonly int _id;
        private readonly string _username;
        private readonly string _passwordHash;

        //protected so only User itself and its subclasses can call this constructor
        protected User(int id, string username, string passwordHash)
        {
            _id = id;
            _username = username;
            _passwordHash = passwordHash;
        }

        // Read-only public properties - outside code can read these, never set them directly
        public int Id => _id;
        public string Username => _username;

        public string PasswordHash => _passwordHash;

        // each subclass must say what role it is — this is what lets
        // UserRepository know whether to build a ClientUser or AdminUser
        // when reading a line back from the file
        public abstract UserRole Role { get; }


        //every subclass must implement its own menu 
        public abstract void DisplayMenu();
    }
}