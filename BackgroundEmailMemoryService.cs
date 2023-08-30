using System.Threading;

namespace AspNetCoreFirstApp
{
    public class BackgroundEmailMemoryService : BackgroundService
    {
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCheck _memoryCheck;
        private readonly TimeSpan _timeout;
        public BackgroundEmailMemoryService(IEmailSender emailSender,
            TimeSpan timeout, IMemoryCheck memoryCheck)
        {
            _emailSender = emailSender;
            _timeout = timeout;
            _memoryCheck = memoryCheck;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _emailSender.SendEmailAsync("Aleksey S.", "asp2022pd011@rodion-m.ru",
                        "6WU4x2be", "Alex", "kokotiv3@ya.ru", "Memory",
                        _memoryCheck.MemoryUsedByApplication(), false, stoppingToken);
                    await Task.Delay(_timeout, stoppingToken);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

    }
}
