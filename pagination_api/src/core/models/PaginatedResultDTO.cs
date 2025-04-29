namespace PaginationApp.Core.Models
{
    // Representa una respuesta paginada genérica con metainformación
    public class PaginatedResult<T>
    {
        public long Total { get; set; }               
        public int PageNumber { get; set; }           
        public int PageSize { get; set; }             
        public List<T> Items { get; set; } = new();   
    }
}
