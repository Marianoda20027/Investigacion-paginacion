using PaginationApp.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaginationApp.Services.ElasticSearch.Contracts;
using PaginationApp.Services.Parts.Contracts;

namespace PaginationApp.Services.Parts
{
    /// <summary>
    /// Implementación concreta del servicio de búsqueda usando Elasticsearch
    /// </summary>
    public class ElasticPartSearchService : IPartSearchService
    {
        private readonly IElasticSearchService _searchService; // Servicio de bajo nivel para Elasticsearch
        private readonly IPartMapper _mapper; // Mapeador de respuestas de Elasticsearch

        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public ElasticPartSearchService(
            IElasticSearchService searchService,
            IPartMapper mapper)
        {
            _searchService = searchService;
            _mapper = mapper;
        }

        /// <summary>
        /// Busca partes paginadas con filtros opcionales
        /// </summary>
        /// <param name="pageNumber">Número de página (1-based)</param>
        /// <param name="pageSize">Cantidad de ítems por página</param>
        /// <param name="filters">Diccionario de filtros aplicables</param>
        /// <returns>Resultado paginado con objetos PartDto</returns>
        public async Task<PaginatedResult<PartDto>> SearchPartsAsync(
            int pageNumber, 
            int pageSize, 
            Dictionary<string, string>? filters = null)
        {
            // 1. Ejecutar búsqueda en Elasticsearch
            var response = await _searchService.SearchPartsAsync(
                filters ?? new Dictionary<string, string>(), // Filtros o diccionario vacío
                pageNumber, 
                pageSize);
                
            // 2. Mapear respuesta de Elasticsearch a DTOs paginados
            return _mapper.MapToPaginatedResult(response, pageNumber, pageSize);
        }
    }
}