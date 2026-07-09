namespace Core.Interfaces
{
    // contract for hashing/verifying passwords. Core doesn't know or care
    // which algorithm is used (BCrypt, SHA256, etc.) — just that something
    // can turn a plain password into a hash, and check a password against one
    public interface IPasswordHasher
    {
        string Hash(string plainPassword);
        bool Verify(string plainPassword, string hashedPassword);
    }
}