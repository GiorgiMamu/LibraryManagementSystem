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
        public BorrowStatus Status { get; private set; }

        public BorrowRecord(string borrowId, int userId, string isbn, DateTime dueDate, BorrowStatus status)
        {
            BorrowId = borrowId;
            UserId = userId;
            Isbn = isbn;
            DueDate = dueDate;
            Status = status;
        }

        public void Approve() => Status = BorrowStatus.Approved;
        public void Reject() => Status = BorrowStatus.Rejected;
        public void MarkReturned() => Status = BorrowStatus.Returned;
        public void MarkOverdue() => Status = BorrowStatus.Overdue;

        public bool IsOverdue(DateTime today) =>
            Status == BorrowStatus.Approved && today.Date > DueDate.Date;

        public bool IsDueTomorrow(DateTime today) =>
            Status == BorrowStatus.Approved && DueDate.Date == today.Date.AddDays(1);
    }
}