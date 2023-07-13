using Microsoft.VisualBasic;
using System.Globalization;

namespace AspNetCoreFirstApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            //app.MapGet("/", () => DateTime.UtcNow.ToString() + " ���� �� �������");
            //app.MapGet("/new_year", () => Math.Round((DateTime.Parse($"01.01.{DateTime.Now.Year + 1}") - DateTime.Now)
            //.TotalDays).ToString() + " ���� �������� �� ������ ����");

            //1
            app.MapGet("/customs_duty", (double price) =>
            {
                string message = "�������� ��������� ������� � ��������: ";
                int limit = 200;
                if (price <= limit)
                    return message + price.ToString() + " �";
                return message + (price += price * 0.15) + " �";
            });

            //2
            app.MapGet("/date_time", (string language) =>
            {
                try
                {
                    DateTime now = DateTime.Now;
                    CultureInfo culture = CultureInfo.GetCultureInfo(language);
                    DateTimeFormatInfo dateTimeFormatInfo = culture.DateTimeFormat;

                    return now.ToString(dateTimeFormatInfo.FullDateTimePattern, culture);
                }
                catch (CultureNotFoundException ex)
                {

                    return ex.Message;
                }
            });

            app.Run();
        }
    }
}