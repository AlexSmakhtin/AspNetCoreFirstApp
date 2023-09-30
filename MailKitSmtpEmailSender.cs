using Microsoft.Extensions.Options;
using MimeKit;

namespace AspNetCoreFirstApp
{
    public class MailKitSmtpEmailSender : IEmailSender, IAsyncDisposable, IDisposable
    {
        private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        private readonly SmtpConfig _options;
        private readonly MailKit.Net.Smtp.SmtpClient _smtpClient;
        private readonly ILogger<MailKitSmtpEmailSender> _logger;
        private bool disposed;
        public MailKitSmtpEmailSender(
            IOptionsSnapshot<SmtpConfig> options,
            ILogger<MailKitSmtpEmailSender> logger)
        {
            ArgumentNullException.ThrowIfNull(options);
            _options = options.Value;
            disposed = false;
            _smtpClient = new MailKit.Net.Smtp.SmtpClient();
            _logger = logger;
            _logger.LogInformation("MailKitSmtpEmailSender created");
        }
        public async Task DisconnectAsync(bool quit)
        {
            await semaphoreSlim.WaitAsync();
            if (_smtpClient.IsConnected == true)
            {
                await _smtpClient.DisconnectAsync(quit);
                _logger.LogInformation("_smtpClient disconnected");
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
            _logger.LogInformation("MailKitSmtpEmailSender disposed");
        }

        public async Task SendEmailAsync(
            string fromName,
            string fromEmail,
            string toName,
            string toEmail,
            string subject,
            string body,
            int retryCount,
            CancellationToken token)
        {
            if (retryCount > 0)
                try
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
                    _logger.LogInformation("Message sent to {@To}", mimeMessage.To.First().Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error");
                    await SendEmailAsync(
                        fromName,
                        fromEmail,
                        toName,
                        toEmail,
                        subject,
                        body,
                        --retryCount,
                        token);
                }
            else
                throw new Exception();
        }

        private async Task EnsureConnectedAndAuthenticated(CancellationToken token)
        {
            await semaphoreSlim.WaitAsync(token);
            try
            {

                if (!_smtpClient.IsConnected)
                {
                    await _smtpClient.ConnectAsync(
                        _options.Host,
                        _options.Port,
                        _options.UseSsl,
                        cancellationToken: token);
                    _logger.LogInformation("_smtpClient connected");
                }
                if (!_smtpClient.IsAuthenticated)
                {
                    await _smtpClient.AuthenticateAsync(
                        _options.Login,
                        _options.Password,
                        cancellationToken: token);
                    _logger.LogInformation("_smtpClient authenticated");
                }

            }
            catch (Exception)
            {
                _logger.LogError("Failed to connect and to authenticate");
                throw;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public void Dispose()
        {
            Dispose(true).Wait();
        }
    }
}

