using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Serilog;

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
                        IConfiguration configuration,
                        CancellationToken token,
                        IOptionsSnapshot<SmtpConfig> options) =>
                        {
                            try
                            {
                                var retryCount = 3;
                                await emailSender.SendEmailAsync(
                                "Aleksey S.",
                                "asp2022pd011@rodion-m.ru",
                                "Alex", "kokotiv3@ya.ru",
                                "Memory",
                                options.Value.EmailText,
                                retryCount,
                                token);
                                return "Сообщение отправлено";
                            }
                            catch (Exception)
                            {
                                return "Сообщение не отправлено";
                            }
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
