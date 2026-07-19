using Core.Enums;
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


    // thrown when the book doesn't exist at all
    public class BookNotFoundException : Exception
    {
        public BookNotFoundException(string isbn)
            : base($"No book found with ISBN '{isbn}'.") { }
    }


    // thrown when no such borrow record exists
    public class BorrowRecordNotFoundException : Exception
    {
        public BorrowRecordNotFoundException(string borrowId)
            : base($"No borrow record found with ID '{borrowId}'.") { }
    }

    // thrown when this borrow record belongs to a different client
    public class UnauthorizedBorrowAccessException : Exception
    {
        public UnauthorizedBorrowAccessException(string borrowId)
            : base($"Borrow record '{borrowId}' does not belong to you.") { }
    }


    // thrown when trying to return something not currently on loan
    // (already returned, still pending, or rejected)
    public class InvalidBorrowStateException : Exception
    {
        public InvalidBorrowStateException(string borrowId, BorrowStatus currentStatus)
            : base($"Borrow record '{borrowId}' cannot be returned (current status: {currentStatus}).") { }
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
            : base($" '{username}' is already taken.") { }
    }

    // thrown when a user tries to borrow a book but has an outstanding fine
    public class FineOutstandingException : Exception
    {
        public FineOutstandingException(decimal amount)
            : base($"Cannot borrow: outstanding fine of {amount:F2} must be paid first.") { }
    }

    // thrown if login is attempted before the account is verified
    public class AccountNotVerifiedException : Exception
    {
        public AccountNotVerifiedException(string username)
            : base($"Account '{username}' is not verified yet. Please enter your verification code first.") { }
    }

    // thrown when the verification code entered doesn't match
    public class InvalidVerificationCodeException : Exception
    {
        public InvalidVerificationCodeException()
            : base("The verification code entered is incorrect.") { }
    }

    // thrown when a client tries to pay a fine but has nothing owed
    public class NoOutstandingFineException : Exception
    {
        public NoOutstandingFineException()
            : base("You have no outstanding fines to pay.") { }
    }
}