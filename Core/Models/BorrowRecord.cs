using System;
using Core.Enums;

namespace Core.Models
{
    public class BorrowRecord
    {
        public string BorrowId { get; }
        public int UserId { get; }
        public string Isbn { get; }
        public DateTime? DueDate { get; private set; }
        public BorrowStatus Status { get; private set; }

        // Tracks the last date fines were charged up to, so repeated overdue
        // checks don't double-charge the same days. Null until the first fine is applied
        public DateTime? FineLastCalculatedDate { get; private set; }

        public BorrowRecord(string borrowId, int userId, string isbn, DateTime? dueDate,
            BorrowStatus status, DateTime? fineLastCalculatedDate = null)
        {
            BorrowId = borrowId;
            UserId = userId;
            Isbn = isbn;
            DueDate = dueDate;
            Status = status;
            FineLastCalculatedDate = fineLastCalculatedDate;
        }

        public void Approve(DateTime dueDate)
        {
            Status = BorrowStatus.Approved;
            DueDate = dueDate;
        }

        public void Reject() => Status = BorrowStatus.Rejected;
        public void MarkReturned() => Status = BorrowStatus.Returned;
        public void MarkOverdue() => Status = BorrowStatus.Overdue;

        public void SetFineLastCalculatedDate(DateTime date) => FineLastCalculatedDate = date;

        public bool IsPastDue(DateTime today) =>
            DueDate.HasValue && today.Date > DueDate.Value.Date;

        public bool IsOverdue(DateTime today) =>
            (Status == BorrowStatus.Approved || Status == BorrowStatus.Overdue) && IsPastDue(today);

        public bool IsDueTomorrow(DateTime today) =>
            Status == BorrowStatus.Approved && DueDate.HasValue && DueDate.Value.Date == today.Date.AddDays(1);
    }
}