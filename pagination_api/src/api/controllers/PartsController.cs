using Microsoft.AspNetCore.Mvc;
using PaginationApp.Core.Exceptions;
using PaginationApp.Core.Utilities.Validators;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaginationApp.Services.Parts.Contracts;

namespace PaginationApp.Api.Controllers
{
    // Controlador principal para gestión de partes con paginación
    [Route("api/[controller]")]
    [ApiController]
    public class PartsController : ControllerBase
    {
        // Servicios inyectados
        private readonly IPartSearchService _partSearchService;  // Servicio de búsqueda en ElasticSearch
        private readonly ILogger<PartsController> _logger;       // Logger para seguimiento de errores

        public PartsController(IPartSearchService partSearchService, ILogger<PartsController> logger)
        {
            _partSearchService = partSearchService;
            _logger = logger;
        }

        /* Endpoint principal para búsqueda paginada con filtros opcionales
           - Todos los parámetros son opcionales con valores por defecto
           - Soporta filtrado por múltiples criterios
           - Incluye validación automática de parámetros */
        [HttpGet]
        public async Task<IActionResult> GetPaginatedParts(
            [FromQuery] int pageNumber = 1,      // Número de página (default 1)
            [FromQuery] int pageSize = 10,       // Items por página (default 10)
            /* Filtros de búsqueda opcionales */
            [FromQuery] string? category = null,
            [FromQuery] string? partCode = null,
            [FromQuery] string? technicalSpecs = null,
            [FromQuery] int? minStockQuantity = null,   // Filtros de rango para stock
            [FromQuery] int? maxStockQuantity = null,
            [FromQuery] decimal? minUnitWeight = null,  // Filtros de rango para peso
            [FromQuery] decimal? maxUnitWeight = null,
            [FromQuery] string? productionDateStart = null,  // Rango fechas de producción
            [FromQuery] string? productionDateEnd = null,
            [FromQuery] string? lastModifiedStart = null,   // Rango fechas de modificación
            [FromQuery] string? lastModifiedEnd = null)
        {
            try
            {
                // Validación de parámetros de entrada
                PartValidator.ValidateSearchParams(
                    pageNumber,
                    pageSize,
                    minStockQuantity,
                    maxStockQuantity,
                    minUnitWeight,
                    maxUnitWeight,
                    productionDateStart,
                    productionDateEnd,
                    lastModifiedStart,
                    lastModifiedEnd
                );

                // Construcción de diccionario de filtros
                var filters = new Dictionary<string, string>();
                
                // Filtros de texto (con normalización)
                if (!string.IsNullOrEmpty(category))
                    filters.Add("category", category.ToLower());
                
                if (!string.IsNullOrEmpty(partCode))
                    filters.Add("partcode", partCode.Trim().ToUpper());

                // Filtros de texto plano
                if (!string.IsNullOrEmpty(technicalSpecs))
                    filters.Add("technicalspecs", technicalSpecs);
                
                // Filtros de rango numérico (solo si tienen valor)
                if (minStockQuantity.HasValue)
                    filters.Add("stockquantity_gte", minStockQuantity.Value.ToString());
                
                if (maxStockQuantity.HasValue)
                    filters.Add("stockquantity_lte", maxStockQuantity.Value.ToString());
                
                // Filtros de rango decimal
                if (minUnitWeight.HasValue)
                    filters.Add("unitweight_gte", minUnitWeight.Value.ToString());
                
                if (maxUnitWeight.HasValue)
                    filters.Add("unitweight_lte", maxUnitWeight.Value.ToString());
                
                // Filtros de rango de fechas
                if (!string.IsNullOrEmpty(productionDateStart))
                    filters.Add("productiondate_gte", productionDateStart);
                
                if (!string.IsNullOrEmpty(productionDateEnd))
                    filters.Add("productiondate_lte", productionDateEnd);
                
                if (!string.IsNullOrEmpty(lastModifiedStart))
                    filters.Add("lastmodified_gte", lastModifiedStart);
                
                if (!string.IsNullOrEmpty(lastModifiedEnd))
                    filters.Add("lastmodified_lte", lastModifiedEnd);

                // Ejecutar búsqueda paginada
                var result = await _partSearchService.SearchPartsAsync(pageNumber, pageSize, filters);
                
                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                // Captura de errores de validación (400 Bad Request)
                _logger.LogWarning(ex, "Bad request in parts search");
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                // Captura de errores inesperados (500 Internal Server Error)
                _logger.LogError(ex, "Error searching parts");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }
    }
}