using System;
using System.Collections.Generic;
using Core.Models;

namespace Services.Interfaces
{
    public interface IBorrowService
    {
        BorrowRecord RequestBorrow(ClientUser client, string isbn);
        void ApproveRequest(string borrowId, DateTime dueDate);
        void RejectRequest(string borrowId);
        void ReturnBook(ClientUser client, string borrowId);
        void PayFine(ClientUser client, decimal amount); 
        List<BorrowRecord> GetPendingRequests();
        List<BorrowRecord> GetAllBorrowRecords();
        List<BorrowRecord> GetBorrowedBooksForClient(int clientId);
        List<BorrowRecord> GetReturnableBooksForClient(int clientId); 
        List<BorrowRecord> CheckAndMarkOverdue(); 
    }
}