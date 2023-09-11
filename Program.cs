using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace AspNetCoreFirstApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddOptions<SmtpConfig>()
                .BindConfiguration("SmtpConfig")
                .ValidateDataAnnotations()
                .ValidateOnStart();
            builder.Services.AddControllers();
            builder.Configuration.AddEnvironmentVariables().Build();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IEmailSender, MailKitSmtpEmailSender>(serviceProvider =>
                new MailKitSmtpEmailSender(serviceProvider.GetService<IOptionsSnapshot<SmtpConfig>>()
                ?? throw new ArgumentNullException("IOptionsSnapshot<SmtpConfig>")));
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
                            await emailSender.SendEmailAsync(
                            "Aleksey S.",
                            "asp2022pd011@rodion-m.ru",
                            "Alex", "kokotiv3@ya.ru",
                            "Memory",
                            options.Value.EmailText,
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
    }
}
