namespace AspNetCoreFirstApp
{
    public interface IEmailSender
    {
        public Task SendEmailAsync(string fromName, string fromEmail,
            string password,
            string toName, string toEmail,
            string subject, string body, bool useSsl, CancellationToken token);
    }
}