namespace PaginationApp.Core.Exceptions
{
    // Excepción utilizada para errores relacionados con configuraciones inválidas
    public class ConfigurationException : AppException
    {
        public ConfigurationException(string message) 
            : base(message, 500) { }
    }
}
