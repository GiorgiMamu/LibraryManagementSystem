using System;
using Core.Enums;

namespace Core.Models
{
    public class BorrowRecord
    {
        public string BorrowId { get; }
        public int UserId { get; }
        public string Isbn { get; }
        public DateTime DueDate { get; }

        // private setter: status can only change via the methods below,
        // never assigned directly from outside this class
        public BorrowStatus Status { get; private set; }

        public BorrowRecord(string borrowId, int userId, string isbn, DateTime dueDate, BorrowStatus status)
        {
            BorrowId = borrowId;
            UserId = userId;
            Isbn = isbn;
            DueDate = dueDate;
            Status = status;
        }

        // these four are the only legal state transitions for a borrow record.
        public void Approve() => Status = BorrowStatus.Approved;
        public void Reject() => Status = BorrowStatus.Rejected;
        public void MarkReturned() => Status = BorrowStatus.Returned;
        public void MarkOverdue() => Status = BorrowStatus.Overdue;


        // convenience methods to check the status of a borrow record
        public bool IsOverdue(DateTime today) =>
            Status == BorrowStatus.Approved && today.Date > DueDate.Date;

        // convenience method to check if the borrow record is due tomorrow
        public bool IsDueTomorrow(DateTime today) =>
            Status == BorrowStatus.Approved && DueDate.Date == today.Date.AddDays(1);
    }
}