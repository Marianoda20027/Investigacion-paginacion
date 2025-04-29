namespace PaginationApp.Core.Exceptions
{
    public class InvalidOperationException : AppException
    {
        public InvalidOperationException(string message)
            : base(message, 500) { }
    }
}
