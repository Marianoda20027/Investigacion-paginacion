using PaginationApp.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaginationApp.Services.Parts.Contracts
{
    public interface IPartSearchService
    {
        Task<PaginatedResult<PartDto>> SearchPartsAsync(
            int pageNumber, 
            int pageSize, 
            Dictionary<string, string>? filters = null);
    }
}