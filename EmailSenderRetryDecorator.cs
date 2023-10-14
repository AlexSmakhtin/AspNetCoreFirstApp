using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Threading;

namespace AspNetCoreFirstApp
{
    public class EmailSenderRetryDecorator : IEmailSender
    {
        private readonly IEmailSender _inner;
        private readonly ILogger<EmailSenderRetryDecorator> _logger;
        private readonly IOptionsSnapshot<SmtpConfig> _options;
        private readonly AsyncRetryPolicy _policy;
        private readonly TimeSpan _timeout;
        public EmailSenderRetryDecorator(
            IEmailSender inner,
            ILogger<EmailSenderRetryDecorator> logger,
            IOptionsSnapshot<SmtpConfig> options)
        {
            _options = options;
            _timeout = TimeSpan.FromMilliseconds(_options.Value.WaitForNextTry);
            _inner = inner;
            _logger = logger;
            _policy = Policy
                .Handle<ConnectionException>()
                .WaitAndRetryAsync(_options.Value.RetryCount, t => _timeout,
                           (ex, timespan, retryAttempt, context) =>
                           {
                               _logger.LogWarning(ex, "Caught an error. Retrying: {attempt}", retryAttempt);
                           });
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
            var result = await _policy
                .ExecuteAndCaptureAsync(() => _inner.SendEmailAsync(
                                            fromName, fromEmail, toName, toEmail, subject, body, token));
            if (result.Outcome == OutcomeType.Failure)
            {
                _logger.LogError(result.FinalException, "There was an error while sending email");
                throw result.FinalException;
            }
        }
    }
}
