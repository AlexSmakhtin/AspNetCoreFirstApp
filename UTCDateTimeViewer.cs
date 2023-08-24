namespace AspNetCoreFirstApp
{
    public class UTCDateTimeViewer : IDateViewer
    {
        public string DateTimeNow(string format)
        {
            return DateTime.UtcNow.ToLocalTime().ToString(format);
        }
    }
}
