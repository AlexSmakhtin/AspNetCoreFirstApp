using Polly;
using Polly.Retry;

namespace AspNetCoreFirstApp
{


    public class EmailSenderRetryDecorator : IEmailSender
    {
        private readonly IEmailSender _inner;
        private readonly ILogger<EmailSenderRetryDecorator> _logger;
        private readonly IConfiguration _configuration;
        private readonly AsyncRetryPolicy _policy;
        public EmailSenderRetryDecorator(
            IEmailSender inner,
            ILogger<EmailSenderRetryDecorator> logger,
            IConfiguration configuration)
        {
            _configuration = configuration;
            _inner = inner;
            _logger = logger;
            _policy = Policy
                .Handle<ConnectionException>()
                .RetryAsync(_configuration
                                .GetSection("SmtpConfig")
                                .GetValue<int>("MaxRetryCount"),
                           (ex, retryAttempt) =>
                                _logger.LogWarning(ex, "Caught an error. Retrying: {attempt}", retryAttempt));
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
                throw result.FinalException;
        }
    }
}
