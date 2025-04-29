namespace PaginationApp.Core.Exceptions
{
    // Excepción específica para errores 400 (Bad Request)
    public class BadRequestException : AppException
    {
        public BadRequestException(string message) : base(message, 400) { }
    }
}
