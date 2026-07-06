using System;

namespace Core.Exceptions
{
    public class BookNotAvailableException : Exception
    {
        public BookNotAvailableException(string isbn)
            : base($"Book with ISBN '{isbn}' is not available") { }
    }

    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string username)
            : base($"User '{username}' was not found.") { }
    }

    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException()
            : base("Invalid username or password.") { }
    }

    public class InsufficientQuantityException : Exception
    {
        public InsufficientQuantityException(string isbn)
            : base($"Cannot decrease quantity for '{isbn}' below zero.") { }
    }

    public class DuplicateUserException : Exception
    {
        public DuplicateUserException(string username)
            : base($"Username '{username}' is already taken.") { }
    }

    public class FineOutstandingException : Exception
    {
        public FineOutstandingException(decimal amount)
            : base($"Cannot borrow: outstanding fine of {amount} must be paid first.") { }
    }
}