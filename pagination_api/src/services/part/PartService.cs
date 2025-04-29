using PaginationApp.Core.Entities;
using PaginationApp.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using PaginationApp.Core.Models;
using PaginationApp.Services.Parts.Contracts;

namespace PaginationApp.Services.Parts
{
    // Servicio principal que orquesta la búsqueda paginada de partes
    public class PartService
    {
        private readonly IPartSearchService _partSearchService;

        public PartService(IPartSearchService partSearchService)
        {
            _partSearchService = partSearchService;
        }

        // Obtiene una lista paginada de partes aplicando filtros opcionales
        public async Task<PaginatedResult<PartDto>> GetPaginatedPartsAsync(
            int pageNumber,
            int pageSize,
            Dictionary<string, string>? filters = null)
        {
            return await _partSearchService.SearchPartsAsync(pageNumber, pageSize, filters);
        }
    }
}
