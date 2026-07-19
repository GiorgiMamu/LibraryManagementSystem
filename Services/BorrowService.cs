using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using Core.Exceptions;
using Core.Interfaces;
using Core.Models;
using Services.Interfaces;

namespace Services
{
    public class BorrowService : IBorrowService
    {
        private const decimal FinePerDay = 0.5m;

        private readonly IBorrowRepository _borrowRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public BorrowService(
            IBorrowRepository borrowRepository,
            IBookRepository bookRepository,
            IUserRepository userRepository,
            ILogger logger)
        {
            _borrowRepository = borrowRepository;
            _bookRepository = bookRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public BorrowRecord RequestBorrow(ClientUser client, string isbn)
        {
            if (!client.CanBorrow())
                throw new FineOutstandingException(client.Fines);

            var book = _bookRepository.GetByIsbn(isbn) ?? throw new BookNotFoundException(isbn);
            if (!book.IsAvailable)
                throw new BookNotAvailableException(isbn);

            var record = new BorrowRecord(
                $"B{DateTime.Now.Ticks % 100000}",
                client.Id,
                isbn,
                null,
                BorrowStatus.Pending);

            _borrowRepository.Add(record);
            _logger.LogInfo($"Borrow request created: {record.BorrowId} by user {client.Id}");
            return record;
        }

        public void ApproveRequest(string borrowId, DateTime dueDate)
        {
            if (dueDate.Date <= DateTime.Now.Date)
                throw new ArgumentException("Due date must be in the future.");

            var record = _borrowRepository.GetById(borrowId) ?? throw new BorrowRecordNotFoundException(borrowId);
            var book = _bookRepository.GetByIsbn(record.Isbn) ?? throw new BookNotFoundException(record.Isbn);

            book.Decrease();
            _bookRepository.Update(book);

            record.Approve(dueDate);
            _borrowRepository.Update(record);
            _logger.LogInfo($"Borrow approved: {borrowId}, due {dueDate:yyyy-MM-dd}");
        }

        public void RejectRequest(string borrowId)
        {
            var record = _borrowRepository.GetById(borrowId) ?? throw new BorrowRecordNotFoundException(borrowId);
            record.Reject();
            _borrowRepository.Update(record);
            _logger.LogInfo($"Borrow rejected: {borrowId}");
        }

        public void ReturnBook(ClientUser client, string borrowId)
        {
            var record = _borrowRepository.GetById(borrowId) ?? throw new BorrowRecordNotFoundException(borrowId);

            if (record.UserId != client.Id)
                throw new UnauthorizedBorrowAccessException(borrowId);

            if (record.Status != BorrowStatus.Approved && record.Status != BorrowStatus.Overdue)
                throw new InvalidBorrowStateException(borrowId, record.Status);

            // Catches up any days that haven't been fined yet ( admin
            // hasn't run the overdue check since this became late) before returning
            AccrueFineIfNeeded(record, client);

            var book = _bookRepository.GetByIsbn(record.Isbn);
            if (book != null)
            {
                book.Increase();
                _bookRepository.Update(book);
            }

            record.MarkReturned();
            _borrowRepository.Update(record);
            _logger.LogInfo($"Book returned: {borrowId}");
        }

        public List<BorrowRecord> GetPendingRequests() =>
            _borrowRepository.GetAll().Where(r => r.Status == BorrowStatus.Pending).ToList();

        public List<BorrowRecord> GetAllBorrowRecords() => _borrowRepository.GetAll();

        public List<BorrowRecord> GetBorrowedBooksForClient(int clientId) =>
            _borrowRepository.GetByUserId(clientId);

        // only records the client can actually act on right now 
        // Approved (on time) or Overdue (still returnable, just late)
        public List<BorrowRecord> GetReturnableBooksForClient(int clientId) =>
            _borrowRepository.GetByUserId(clientId)
                .Where(r => r.Status == BorrowStatus.Approved || r.Status == BorrowStatus.Overdue)
                .ToList();

        // run by the admin. Marks anything overdue and charges a fine for every elapsed day since the last time
        // this ran (or since the due date, whichever is more recent) 
        public List<BorrowRecord> CheckAndMarkOverdue()
        {
            var today = DateTime.Now;
            var affected = new List<BorrowRecord>();

            foreach (var record in _borrowRepository.GetAll())
            {
                if (record.Status != BorrowStatus.Approved && record.Status != BorrowStatus.Overdue)
                    continue;
                if (!record.IsPastDue(today))
                    continue;

                bool changed = false;

                if (record.Status == BorrowStatus.Approved)
                {
                    record.MarkOverdue();
                    changed = true;
                    _logger.LogWarning($"Borrow {record.BorrowId} automatically marked Overdue.");
                }

                if (_userRepository.GetById(record.UserId) is ClientUser client)
                    changed |= AccrueFineIfNeeded(record, client); 

                if (changed)
                    _borrowRepository.Update(record);

                affected.Add(record);
            }
            return affected;
        }
        
        //path that reduces a fine 
        public void PayFine(ClientUser client, decimal amount)
        {
            if (client.Fines == 0)
                throw new NoOutstandingFineException();

            if (amount <= 0)
                throw new ArgumentException("Payment amount must be a positive number.");

            client.PayFine(amount);
            _userRepository.Update(client);
            _logger.LogInfo($"User {client.Id} paid {amount:F2} towards fines. Remaining: {client.Fines:F2}");
        }

        
        // returns true if a fine was actually added (so callers know whether
        // the record needs to be persisted).
        private bool AccrueFineIfNeeded(BorrowRecord record, ClientUser client)
        {
            var today = DateTime.Now;
            if (!record.IsPastDue(today)) return false;

            var start = record.FineLastCalculatedDate?.Date ?? record.DueDate!.Value.Date;
            int unfinedDays = (today.Date - start).Days;
            if (unfinedDays <= 0) return false;

            client.AddFine(unfinedDays * FinePerDay);
            _userRepository.Update(client);
            record.SetFineLastCalculatedDate(today);

            _logger.LogWarning(
                $"Fine of {unfinedDays * FinePerDay:F2} added to user {client.Id} for {record.BorrowId} ({unfinedDays} day(s) since last check).");
            return true;
        }
    }
}