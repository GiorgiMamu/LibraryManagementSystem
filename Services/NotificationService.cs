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
                    bool emailSent = _emailService.SendEmail(
    user.Email,
    "Reminder: Your Book Is Due Tomorrow",
    $@"
    <div style='max-width:500px;margin:40px auto;padding:30px;
                font-family:Arial,sans-serif;
                border:1px solid #e5e5e5;
                border-radius:10px;
                background:#ffffff;
                text-align:center;'>

        <h2 style='color:#2563eb;margin-bottom:10px;'>
            📚 Library System
        </h2>

        <p style='font-size:16px;color:#444;'>
            Hello <strong>{user.Username}</strong>,
        </p>

        <p style='color:#666;'>
            This is a friendly reminder that your borrowed book is due tomorrow.
        </p>

        <div style='background:#f3f7ff;
                    padding:15px;
                    border-radius:8px;
                    margin:25px 0;
                    color:#333;'>
            <strong>Book:</strong> {title}<br>
            <strong>Due Date:</strong> {record.DueDate:yyyy-MM-dd}
        </div>

        <p style='color:#666;'>
            Please return it on time to avoid any late fees.
        </p>

    </div>");

                    if (emailSent)
                    {
                        sent.Add($"Due-tomorrow reminder sent to {user.Email} for \"{title}\"");
                        _logger.LogInfo($"Due-tomorrow email sent to {user.Email} for {record.BorrowId}");
                    }
                    else
                    {
                        sent.Add($"FAILED to send due-tomorrow reminder to {user.Email} for \"{title}\"");
                        _logger.LogWarning($"Failed to send due-tomorrow email to {user.Email} for {record.BorrowId}");
                    }
                }
                else if (record.IsOverdue(today))
                {
                    var daysLate = (today.Date - record.DueDate!.Value.Date).Days;

                    bool emailSent = _emailService.SendEmail(
      user.Email,
      "Overdue: Your Library Book Is Late",
      $@"
    <div style='max-width:500px;margin:40px auto;padding:30px;
                font-family:Arial,sans-serif;
                border:1px solid #e5e5e5;
                border-radius:10px;
                background:#ffffff;
                text-align:center;'>

        <h2 style='color:#d32f2f;margin-bottom:10px;'>
            📚 Library System
        </h2>

        <p style='font-size:16px;color:#444;'>
            Hello <strong>{user.Username}</strong>,
        </p>

        <p style='color:#666;'>
            Your borrowed book is now overdue.
        </p>

        <div style='background:#fff4f4;
                    padding:15px;
                    border-radius:8px;
                    margin:25px 0;
                    color:#333;'>
            <strong>Book:</strong> {title}<br>
            <strong>Due Date:</strong> {record.DueDate:yyyy-MM-dd}<br>
            <strong>Days Overdue:</strong> {daysLate}
        </div>

        <p style='color:#d32f2f;font-weight:bold;'>
            A late fee is accruing until the book is returned.
        </p>

    </div>");

                    if (emailSent)
                    {
                        sent.Add($"Overdue notice sent to {user.Email} for \"{title}\" ({daysLate} day(s) late)");
                        _logger.LogWarning($"Overdue email sent to {user.Email} for {record.BorrowId}");
                    }
                    else
                    {
                        sent.Add($"FAILED to send overdue notice to {user.Email} for \"{title}\" ({daysLate} day(s) late)");
                        _logger.LogWarning($"Failed to send overdue email to {user.Email} for {record.BorrowId}");
                    }
                }
            }
            return sent;
        }
    }
}