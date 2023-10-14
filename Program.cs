using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using System.Text;
using Polly;

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
                builder.Services.AddOptions<SmtpConfig>()
                   .BindConfiguration("SmtpConfig")
                   .ValidateDataAnnotations()
                   .ValidateOnStart();
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
                builder.Services.Decorate<IEmailSender, EmailSenderRetryDecorator>();

                var app = builder.Build();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                    options.RoutePrefix = string.Empty;
                });
                app.MapGet("/send_email",
                    async (
                        IEmailSender emailSenderRetryDecorator,
                        CancellationToken token,
                        IOptionsSnapshot<SmtpConfig> options,
                        ILogger<Program> logger) =>
                        {
                            logger.LogInformation("Sending...");
                            await emailSenderRetryDecorator.SendEmailAsync(
                               "Aleksey S.",
                               options.Value.Login,
                               "Alex",
                               "kokotiv3@ya.ru",
                               "Memory",
                               options.Value.EmailText,
                               token);
                            logger.LogInformation("Message delivered");
                            return "Сообщение отправлено";
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
