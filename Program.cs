using Serilog;
using System.Text;

namespace AspNetCoreFirstApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            Log.Information("Server started");
            try
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog((ctx, conf) =>
                {
                    conf.MinimumLevel.Information()
                    .WriteTo.Console()
                    .MinimumLevel.Information()
                    .WriteTo.File(
                        "Logs/log.txt",
                        rollingInterval: RollingInterval.Day,
                        encoding: Encoding.UTF8)
                    .MinimumLevel.Information();
                });
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddScoped<IEmailSender, MailKitSmtpEmailSender>();
                var app = builder.Build();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                    options.RoutePrefix = string.Empty;
                });
                app.MapGet("/send_email",
                    async (
                        IEmailSender emailSender,
                        CancellationToken token,
                        IConfiguration config,
                        ILogger<Program> logger) =>
                        {
                            var retryCount = 1;
                            while (retryCount < 4)
                            {
                                try
                                {
                                    logger.LogInformation("Sending... Try: {@try}", retryCount);
                                    await emailSender.SendEmailAsync(
                                       "Aleksey S.",
                                       config.GetSection("SmtpConfig").GetValue<string>("Login")
                                           ?? throw new ArgumentNullException(nameof(config)),
                                       "Alex",
                                       "kokotiv3@ya.ru",
                                       "Memory",
                                       config.GetSection("SmtpConfig").GetValue<string>("EmailText")
                                           ?? throw new ArgumentNullException(nameof(config)),
                                       token);
                                    logger.LogInformation("Message delivered");
                                    return "Сообщение отправлено";
                                }
                                catch (Exception ex)
                                {
                                    logger.LogWarning("Caugth an error: {@ex}", ex);
                                    await Task.Delay(TimeSpan.FromSeconds(5));
                                }
                                finally
                                {
                                    retryCount++;
                                }
                            }
                            logger.LogError(
                                "Error with IEmailSender: message not delivered {@sender}",
                                emailSender);
                            return "Сообщение не отправлено";
                        });
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unexpected error");
            }
            finally
            {
                Log.Information("Server shutted down");
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
