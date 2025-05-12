using Microsoft.AspNetCore.Mvc;
using PaginationApp.Core.Exceptions;
using PaginationApp.Core.Utilities.Validators;
using PaginationApp.Services.Parts;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaginationApp.Services.Parts.Contracts;

namespace PaginationApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartsController : ControllerBase
    {
        private readonly IPartSearchService _partSearchService;
        private readonly ILogger<PartsController> _logger;

        public PartsController(
            IPartSearchService partSearchService,
            ILogger<PartsController> logger)
        {
            _partSearchService = partSearchService;
            _logger = logger;
        }

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
            try
            {
                // Validación centralizada
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

                var filters = new Dictionary<string, string>();
                
                if (!string.IsNullOrEmpty(category))
                    filters.Add("category", category.ToLower());
                
                if (!string.IsNullOrEmpty(partCode))
                {
                    var cleanPartCode = partCode.Trim().ToUpper();
                    filters.Add("partcode", cleanPartCode);
                }
                
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

                // Ejecución de la búsqueda
                var result = await _partSearchService.SearchPartsAsync(pageNumber, pageSize, filters);
                
                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning(ex, "Bad request in parts search");
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching parts");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }
    }
}