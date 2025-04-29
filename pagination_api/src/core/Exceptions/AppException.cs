namespace PaginationApp.Core.Exceptions
{
    // Excepción personalizada que permite incluir un código de estado HTTP
    public class AppException : Exception
    {
        public int StatusCode { get; }

        public AppException(string message, int statusCode = 500) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
