namespace PaginationApp.Core.Exceptions
{
    // Excepción para representar recursos no encontrados (HTTP 404)
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, 404) { }
    }
}
