namespace Core.Interfaces
{
    //contract for sending emails
    public interface IEmailService
    {
        bool SendEmail(string toEmail, string subject, string body);
    }
}