namespace Core.Interfaces
{
    //contract for sending emails
    public interface IEmailService
    {
        void SendEmail(string toEmail, string subject, string body);
    }
}