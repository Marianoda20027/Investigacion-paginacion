using PaginationApp.Core.Models;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using System;
using PaginationApp.Services.Parts.Contracts;

namespace PaginationApp.Services.Parts
{
    // Encargado de transformar la respuesta JSON de Elasticsearch en un objeto paginado con PartDto
    public class PartMapper : IPartMapper
    {
        public PaginatedResult<PartDto> MapToPaginatedResult(string elasticResponse, int pageNumber, int pageSize)
        {
            using var jsonDoc = JsonDocument.Parse(elasticResponse);
            var root = jsonDoc.RootElement;

            // Verifica que exista la propiedad principal 'hits'
            if (!root.TryGetProperty("hits", out var hits))
                throw new InvalidOperationException("Invalid Elasticsearch response format: missing 'hits' property");

            long total = ExtractTotalHits(hits);
            var items = ExtractItems(hits);

            // Construye y retorna el resultado paginado
            return new PaginatedResult<PartDto>
            {
                Items = items,
                Total = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // Extrae el número total de resultados encontrados
        private long ExtractTotalHits(JsonElement hits)
        {
            if (hits.TryGetProperty("total", out var totalProp) &&
                totalProp.TryGetProperty("value", out var totalValue))
            {
                return totalValue.GetInt64();
            }

            return 0;
        }

        // Convierte cada entrada ('hit') en un PartDto
        private List<PartDto> ExtractItems(JsonElement hits)
        {
            var items = new List<PartDto>();

            if (!hits.TryGetProperty("hits", out var hitsArray))
                return items;

            foreach (var hit in hitsArray.EnumerateArray())
            {
                if (!hit.TryGetProperty("_source", out var source))
                    continue;

                var dto = new PartDto
                {
                    Id = source.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0,
                    PartCode = source.TryGetProperty("partcode", out var codeProp) ? codeProp.GetString() : null,
                    Category = source.TryGetProperty("category", out var categoryProp) ? categoryProp.GetString() : null,
                    StockQuantity = source.TryGetProperty("stockquantity", out var stockProp) ? stockProp.GetInt32() : 0,
                    UnitWeight = source.TryGetProperty("unitweight", out var weightProp) ? weightProp.GetDecimal() : 0m,
                    TechnicalSpecs = source.TryGetProperty("technicalspecs", out var specsProp) ? specsProp.GetString() : null,
                    ProductionDate = ParseDate(source, "productiondate"),
                    LastModified = ParseDate(source, "lastmodified")
                };

                items.Add(dto);
            }

            return items;
        }

        // Realiza el parseo seguro de fechas desde una propiedad JSON
        private DateTime? ParseDate(JsonElement source, string propertyName)
        {
            if (source.TryGetProperty(propertyName, out var dateProp) &&
                DateTime.TryParse(dateProp.GetString(), out var parsedDate))
            {
                return parsedDate;
            }

            return null;
        }
    }
}
