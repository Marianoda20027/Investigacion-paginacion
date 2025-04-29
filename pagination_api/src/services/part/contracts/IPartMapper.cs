using PaginationApp.Core.Models;

namespace PaginationApp.Services.Parts.Contracts
{
    public interface IPartMapper
    {
        PaginatedResult<PartDto> MapToPaginatedResult(
            string elasticResponse, 
            int pageNumber, 
            int pageSize);
    }
}