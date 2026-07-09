using System.Collections.Generic;
using Core.Models;

namespace Core.Interfaces
{
    public interface IBorrowRepository
    {
        List<BorrowRecord> GetAll();
        BorrowRecord GetById(string borrowId);
        List<BorrowRecord> GetByUserId(int userId); // my borrow history
        void Add(BorrowRecord record); // new borrow record / request
        void Update(BorrowRecord record); // status changes: approve/reject/return
    }
}