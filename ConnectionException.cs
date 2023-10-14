namespace AspNetCoreFirstApp
{
    public class ConnectionException : Exception
    {
        public ConnectionException(string message) : base(message) { }
    }
}