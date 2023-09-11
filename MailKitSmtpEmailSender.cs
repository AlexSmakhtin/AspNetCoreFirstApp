using Microsoft.Extensions.Options;
using MimeKit;

namespace AspNetCoreFirstApp
{
    public class MailKitSmtpEmailSender : IEmailSender, IAsyncDisposable, IDisposable
    {
        private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        private readonly SmtpConfig _options;
        private readonly MailKit.Net.Smtp.SmtpClient _smtpClient;
        private bool disposed;

        public MailKitSmtpEmailSender(IOptionsSnapshot<SmtpConfig> options)
        {
            ArgumentNullException.ThrowIfNull(options);
            _options = options.Value;
            disposed = false;
            _smtpClient = new MailKit.Net.Smtp.SmtpClient();
        }
        public async Task DisconnectAsync(bool quit)
        {
            await semaphoreSlim.WaitAsync();
            if (_smtpClient.IsConnected == true)
            {
                await _smtpClient.DisconnectAsync(quit);
            }
            semaphoreSlim.Release();
        }
        public async ValueTask DisposeAsync()
        {
            await Dispose(true);
        }
        private async Task Dispose(bool disposing)
        {
            await semaphoreSlim.WaitAsync();
            if (disposed == true)
                return;
            if (disposing)
            {
                if (_smtpClient.IsConnected == true)
                {
                    await _smtpClient.DisconnectAsync(true);
                }
                _smtpClient.Dispose();
            }
            disposed = true;
            semaphoreSlim.Release();
        }

        public async Task SendEmailAsync(
            string fromName,
            string fromEmail,
            string toName,
            string toEmail,
            string subject,
            string body,
            CancellationToken token)
        {
            await EnsureConnectedAndAuthenticated(token);
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(fromName, fromEmail));
            mimeMessage.To.Add(new MailboxAddress(toName, toEmail));
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = body
            };
            await _smtpClient.SendAsync(mimeMessage, token);
        }

        private async Task EnsureConnectedAndAuthenticated(CancellationToken token)
        {
            await semaphoreSlim.WaitAsync(token);
            if (!_smtpClient.IsConnected)
            {
                await _smtpClient.ConnectAsync(
                    _options.Host,
                    _options.Port,
                    _options.UseSsl,
                    cancellationToken: token);
            }
            if (!_smtpClient.IsAuthenticated)
            {
                await _smtpClient.AuthenticateAsync(
                    _options.Login,
                    _options.Password,
                    cancellationToken: token);
            }
            semaphoreSlim.Release();
        }

        public void Dispose()
        {
            Dispose(true).Wait();
        }
    }
}

