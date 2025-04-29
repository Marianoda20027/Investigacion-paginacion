using PaginationApp.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaginationApp.Services.ElasticSearch.Contracts; 
using PaginationApp.Services.Parts.Contracts;

namespace PaginationApp.Services.Parts
{
    // Implementación del servicio de búsqueda de partes usando Elasticsearch
    public class ElasticPartSearchService : IPartSearchService
    {
        private readonly IElasticSearchService _searchService;
        private readonly IPartMapper _mapper;

        // Constructor que recibe las dependencias necesarias
        public ElasticPartSearchService(
            IElasticSearchService searchService, // ✅ Cambio: usar la interfaz registrada
            IPartMapper mapper)
        {
            _searchService = searchService;
            _mapper = mapper;
        }

        // Ejecuta la búsqueda y transforma la respuesta en un resultado paginado tipado
        public async Task<PaginatedResult<PartDto>> SearchPartsAsync(int pageNumber, int pageSize, Dictionary<string, string>? filters = null)
        {
            var response = await _searchService.SearchPartsAsync(filters ?? new Dictionary<string, string>(), pageNumber, pageSize);
            return _mapper.MapToPaginatedResult(response, pageNumber, pageSize);
        }
    }
}
