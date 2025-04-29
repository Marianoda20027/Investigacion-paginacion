using Microsoft.AspNetCore.Mvc;
using PaginationApp.Core.Exceptions;
using PaginationApp.Services.Parts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaginationApp.Api.Controllers
{
    // Controlador que expone el endpoint de consulta paginada de partes
    [Route("api/[controller]")]
    [ApiController]
    public class PartsController : ControllerBase
    {
        private readonly PartService _partService;

        public PartsController(PartService partService)
        {
            _partService = partService;
        }

        // GET api/parts: retorna lista de partes con filtros opcionales y paginación
        [HttpGet]
        public async Task<IActionResult> GetPaginatedParts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? category = null,
            [FromQuery] string? partCode = null,
            [FromQuery] string? technicalSpecs = null,
            [FromQuery] int? minStockQuantity = null,
            [FromQuery] int? maxStockQuantity = null,
            [FromQuery] decimal? minUnitWeight = null,
            [FromQuery] decimal? maxUnitWeight = null,
            [FromQuery] string? productionDateStart = null,
            [FromQuery] string? productionDateEnd = null,
            [FromQuery] string? lastModifiedStart = null,
            [FromQuery] string? lastModifiedEnd = null)
        {
            // Validación de parámetros de paginación
            if (pageNumber < 1)
                throw new BadRequestException("Número de página inválido (debe ser ≥ 1)");

            if (pageSize < 1 || pageSize > 301)
                throw new BadRequestException("Tamaño de página inválido (debe ser 1-300)");

            // Construcción dinámica de filtros en un diccionario
            var filters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(category))
                filters.Add("category", category.ToLower());

            if (!string.IsNullOrEmpty(partCode))
                filters.Add("partcode", partCode);

            if (!string.IsNullOrEmpty(technicalSpecs))
                filters.Add("technicalspecs", technicalSpecs);

            if (minStockQuantity.HasValue)
                filters.Add("stockquantity_gte", minStockQuantity.Value.ToString());

            if (maxStockQuantity.HasValue)
                filters.Add("stockquantity_lte", maxStockQuantity.Value.ToString());

            if (minUnitWeight.HasValue)
                filters.Add("unitweight_gte", minUnitWeight.Value.ToString());

            if (maxUnitWeight.HasValue)
                filters.Add("unitweight_lte", maxUnitWeight.Value.ToString());

            if (!string.IsNullOrEmpty(productionDateStart))
                filters.Add("productiondate_gte", productionDateStart);

            if (!string.IsNullOrEmpty(productionDateEnd))
                filters.Add("productiondate_lte", productionDateEnd);

            if (!string.IsNullOrEmpty(lastModifiedStart))
                filters.Add("lastmodified_gte", lastModifiedStart);

            if (!string.IsNullOrEmpty(lastModifiedEnd))
                filters.Add("lastmodified_lte", lastModifiedEnd);

            // Consulta los datos al servicio con filtros y paginación
            var result = await _partService.GetPaginatedPartsAsync(pageNumber, pageSize, filters);

            return Ok(result);
        }
    }
}
