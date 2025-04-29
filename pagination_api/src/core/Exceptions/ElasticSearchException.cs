namespace PaginationApp.Core.Exceptions
{
    // Excepción usada cuando ocurre un error al interactuar con Elasticsearch
    public class ElasticsearchException : AppException
    {
        public ElasticsearchException(string message) : base(message, 503) { }
    }
}
