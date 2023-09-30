using MimeKit.Encodings;

namespace AspNetCoreFirstApp
{
    public interface IEmailSender
    {
        public Task SendEmailAsync(
            string fromName,
            string fromEmail,
            string toName,
            string toEmail,
            string subject,
            string body,
            int retryCount,
            CancellationToken token);
    }
}