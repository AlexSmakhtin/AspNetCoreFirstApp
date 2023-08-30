using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;

namespace AspNetCoreFirstApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<IMemoryCheck, MemoryChecker>();
            builder.Services.AddTransient<IEmailSender, BegetSMTPEmailSender>(
                serviceProvider => new BegetSMTPEmailSender(LoggerFactory.Create(builder =>
                {
                    builder.AddSimpleConsole(i => i.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled);
                })
                .CreateLogger("Program")));
            builder.Services.AddHostedService(serviceProvider =>
                new BackgroundEmailMemoryService(serviceProvider.GetService<IEmailSender>(),
                    TimeSpan.FromMinutes(60),
                    serviceProvider.GetService<IMemoryCheck>()));
            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
            app.Run();
        }
    }
}
