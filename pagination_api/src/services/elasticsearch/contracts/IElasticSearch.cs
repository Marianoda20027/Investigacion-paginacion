namespace PaginationApp.Services.ElasticSearch.Contracts
{
    public interface IElasticSearchService
    {
        Task<string> SearchPartsAsync(
            Dictionary<string, string> filters, 
            int pageNumber, 
            int pageSize);
    }
}
