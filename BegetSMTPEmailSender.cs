using MailKit.Net.Smtp;
using MimeKit;

namespace AspNetCoreFirstApp
{
    public class BegetSMTPEmailSender : IEmailSender
    {
        private readonly MimeMessage mimeMessage;
        private readonly string smtpServer;
        private readonly int smtpPort;
        private readonly ILogger _logger;
        public BegetSMTPEmailSender(ILogger logger)
        {
            smtpServer = "smtp.beget.com";
            smtpPort = 25;
            mimeMessage = new MimeMessage();
            _logger = logger;
        }
        public async Task SendEmailAsync(string fromName, string fromEmail,
            string password,
            string toName, string toEmail,
            string subject, string body, bool useSsl, CancellationToken token)
        {
            mimeMessage.From.Add(new MailboxAddress(fromName, fromEmail));
            mimeMessage.To.Add(new MailboxAddress(toName, toEmail));
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = body
            };
            try
            {
                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(smtpServer, smtpPort, useSsl, token);
                await smtpClient.AuthenticateAsync(fromEmail, password, token);
                var response = await smtpClient.SendAsync(mimeMessage, token);
                _logger.LogInformation($"Ответ smtp сервера: {response}");
                await smtpClient.DisconnectAsync(true, token);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }

        }
    }
}

