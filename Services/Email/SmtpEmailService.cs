using System;
using System.Net;
using System.Net.Mail;
using Core.Interfaces;
using Services.Interfaces;


namespace Services.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly ILogger _logger;

        public SmtpEmailService(ILogger logger)
        {
            _logger = logger;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                string host = Environment.GetEnvironmentVariable("SMTP_HOST")
                    ?? throw new InvalidOperationException("SMTP_HOST is not set.");
                int port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
                string username = Environment.GetEnvironmentVariable("SMTP_USERNAME")
                    ?? throw new InvalidOperationException("SMTP_USERNAME is not set.");
                string password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")
                    ?? throw new InvalidOperationException("SMTP_PASSWORD is not set.");
                string fromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "Library System";

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                using var mail = new MailMessage
                {
                    From = new MailAddress(username, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                mail.To.Add(toEmail);

                client.Send(mail);
                _logger.LogInfo($"Email sent to {toEmail}: \"{subject}\"");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {toEmail}", ex);
            }
        }
    }
}