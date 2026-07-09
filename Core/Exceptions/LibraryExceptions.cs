using System;

// custom exception classes for the library management system
namespace Core.Exceptions
{
    // thrown when a book is not available for borrowing
    public class BookNotAvailableException : Exception
    {
        public BookNotAvailableException(string isbn)
            : base($"Book with ISBN '{isbn}' is not available") { }
    }

    // thrown when a user is not found in the system
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string username)
            : base($"User '{username}' was not found.") { }
    }

    // thrown when a user attempts to log in with invalid credentials
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException()
            : base("Invalid username or password.") { }
    }

    // thrown if code tries to decrease a book's quantity below zero
    // (a safety net - Book.Decrease() checks this before it happens)
    public class InsufficientQuantityException : Exception
    {
        public InsufficientQuantityException(string isbn)
            : base($"Cannot decrease quantity for '{isbn}' below zero.") { }
    }

    // thrown when a user tries to register with a username that is already taken
    public class DuplicateUserException : Exception
    {
        public DuplicateUserException(string username)
            : base($"Username '{username}' is already taken.") { }
    }

    // thrown when a user tries to borrow a book but has an outstanding fine
    public class FineOutstandingException : Exception
    {
        public FineOutstandingException(decimal amount)
            : base($"Cannot borrow: outstanding fine of {amount:F2} must be paid first.") { }
    }
}