namespace AspNetCoreFirstApp
{
    public class ConnectionException : Exception
    {
        private readonly Exception _parent;
        public ConnectionException(Exception parent)
        {
            _parent = parent;
        }
        public override string Message
        {
            get
            {
                return _parent.Message;
            }
        }
    }
}