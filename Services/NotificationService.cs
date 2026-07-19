using System;
using System.Collections.Generic;
using Core.Interfaces;
using Core.Models;
using Services.Interfaces;

namespace Services
{
    public class NotificationService : INotificationService
    {
        private readonly IBorrowRepository _borrowRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;

        public NotificationService(
            IBorrowRepository borrowRepository,
            IUserRepository userRepository,
            IBookRepository bookRepository,
            IEmailService emailService,
            ILogger logger)
        {
            _borrowRepository = borrowRepository;
            _userRepository = userRepository;
            _bookRepository = bookRepository;
            _emailService = emailService;
            _logger = logger;
        }


        // returns a short summary line per notification for the admin to see
        // in the console after triggering this the actual message goes by email
        public List<string> CheckDueDatesAndNotify()
        {
            var sent = new List<string>();
            var today = DateTime.Now;

            foreach (var record in _borrowRepository.GetAll())
            {
                var user = _userRepository.GetById(record.UserId);
                if (user == null) continue;

                var book = _bookRepository.GetByIsbn(record.Isbn);
                string title = book?.Title ?? record.Isbn;

                if (record.IsDueTomorrow(today))
                {
                    _emailService.SendEmail(
                        user.Email,
                        "Reminder: your book is due tomorrow",
                        $"Hello {user.Username},\n\n\"{title}\" is due back tomorrow ({record.DueDate:yyyy-MM-dd}). Please return it on time to avoid a fine.");

                    sent.Add($"Due-tomorrow reminder sent to {user.Email} for \"{title}\"");
                    _logger.LogInfo($"Due-tomorrow email sent to {user.Email} for {record.BorrowId}");
                }
                else if (record.IsOverdue(today))
                {
                    var daysLate = (today.Date - record.DueDate!.Value.Date).Days;

                    _emailService.SendEmail(
                        user.Email,
                        "Overdue: your library book is late",
                        $"Hello {user.Username},\n\n\"{title}\" was due on {record.DueDate:yyyy-MM-dd} and is now {daysLate} day(s) overdue. A fine is accruing until it's returned.");

                    sent.Add($"Overdue notice sent to {user.Email} for \"{title}\" ({daysLate} day(s) late)");
                    _logger.LogWarning($"Overdue email sent to {user.Email} for {record.BorrowId}");
                }
            }
            return sent;
        }
    }
}