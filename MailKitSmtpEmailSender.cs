using Microsoft.Extensions.Options;
using MimeKit;

namespace AspNetCoreFirstApp
{
    public class MailKitSmtpEmailSender : IEmailSender, IAsyncDisposable, IDisposable
    {
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private readonly IOptionsSnapshot<SmtpConfig> _config;
        private readonly MailKit.Net.Smtp.SmtpClient _smtpClient;
        private readonly ILogger<MailKitSmtpEmailSender> _logger;
        private bool disposed;
        public MailKitSmtpEmailSender(
            IOptionsSnapshot<SmtpConfig> config,
            ILogger<MailKitSmtpEmailSender> logger)
        {
            _config = config;
            disposed = false;
            _smtpClient = new MailKit.Net.Smtp.SmtpClient();
            _logger = logger;
            _logger.LogInformation("MailKitSmtpEmailSender created");
        }
        public async Task DisconnectAsync(bool quit)
        {
            await _semaphoreSlim.WaitAsync();
            if (_smtpClient.IsConnected == true)
            {
                await _smtpClient.DisconnectAsync(quit);
                _logger.LogInformation("_smtpClient disconnected");
            }
            _semaphoreSlim.Release();
        }
        public async ValueTask DisposeAsync()
        {
            await Dispose(true);
        }
        private async Task Dispose(bool disposing)
        {
            await _semaphoreSlim.WaitAsync();
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
            _semaphoreSlim.Release();
            _logger.LogInformation("MailKitSmtpEmailSender disposed");
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
            var response = await _smtpClient.SendAsync(mimeMessage, token);
            _logger.LogInformation("Smtp server response: {@response}", response);
            _logger.LogInformation("Message sent to {@To}", mimeMessage.To.First().Name);

        }

        private async Task EnsureConnectedAndAuthenticated(CancellationToken token)
        {
            try
            {
                await _semaphoreSlim.WaitAsync(token);
                if (!_smtpClient.IsConnected)
                {
                    await _smtpClient.ConnectAsync(
                        _config.Value.Host,
                        _config.Value.Port,
                        _config.Value.UseSsl,
                        cancellationToken: token);
                    _logger.LogInformation("_smtpClient connected");
                }
                if (!_smtpClient.IsAuthenticated)
                {
                    await _smtpClient.AuthenticateAsync(
                        _config.Value.Login,
                        _config.Value.Password,
                        cancellationToken: token);
                    _logger.LogInformation("_smtpClient authenticated");
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public void Dispose()
        {
            Dispose(true).Wait();
        }
    }
}

