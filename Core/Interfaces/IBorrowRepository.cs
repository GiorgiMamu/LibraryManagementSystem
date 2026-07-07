using System.Collections.Generic;
using Core.Models;

namespace Core.Interfaces
{
    public interface IBorrowRepository
    {
        List<BorrowRecord> GetAll();
        BorrowRecord GetById(string borrowId);
        List<BorrowRecord> GetByUserId(int userId);
        void Add(BorrowRecord record);
        void Update(BorrowRecord record);
    }
}