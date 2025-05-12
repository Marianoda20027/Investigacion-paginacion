using PaginationApp.Core.Models;
using System.Text.Json;
using System.Collections.Generic;
using System;
using PaginationApp.Services.Parts.Contracts;

namespace PaginationApp.Services.Parts
{
    /// <summary>
    /// Mapeador de respuestas de Elasticsearch a objetos PartDto paginados
    /// </summary>
    public class PartMapper : IPartMapper
    {
        /// <summary>
        /// Convierte la respuesta JSON de Elasticsearch en un resultado paginado
        /// </summary>
        public PaginatedResult<PartDto> MapToPaginatedResult(string elasticResponse, int pageNumber, int pageSize)
        {
            using var jsonDoc = JsonDocument.Parse(elasticResponse);
            var root = jsonDoc.RootElement;

            // Validar estructura básica de la respuesta
            if (!root.TryGetProperty("hits", out var hits))
                throw new InvalidOperationException("Invalid Elasticsearch response format: missing 'hits' property");

            // Extraer datos principales
            long total = ExtractTotalHits(hits);
            var items = ExtractItems(hits);

            return new PaginatedResult<PartDto>
            {
                Items = items,
                Total = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // Extrae el total de resultados coincidentes
        private long ExtractTotalHits(JsonElement hits)
        {
            if (hits.TryGetProperty("total", out var totalProp) &&
                totalProp.TryGetProperty("value", out var totalValue))
            {
                return totalValue.GetInt64();
            }
            return 0;
        }

        // Convierte los hits de Elasticsearch en objetos PartDto
        private List<PartDto> ExtractItems(JsonElement hits)
        {
            var items = new List<PartDto>();

            if (!hits.TryGetProperty("hits", out var hitsArray))
                return items;

            foreach (var hit in hitsArray.EnumerateArray())
            {
                if (!hit.TryGetProperty("_source", out var source))
                    continue;

                items.Add(MapSourceToDto(source));
            }
            return items;
        }

        // Mapea un documento _source de Elasticsearch a PartDto
        private PartDto MapSourceToDto(JsonElement source)
        {
            return new PartDto
            {
                Id = GetIntProperty(source, "id"),
                PartCode = GetStringProperty(source, "partcode"),
                Category = GetStringProperty(source, "category"),
                StockQuantity = GetIntProperty(source, "stockquantity"),
                UnitWeight = GetDecimalProperty(source, "unitweight"),
                TechnicalSpecs = GetStringProperty(source, "technicalspecs"),
                ProductionDate = ParseDate(source, "productiondate"),
                LastModified = ParseDate(source, "lastmodified")
            };
        }

        // Métodos auxiliares para extraer propiedades con valores por defecto
        private int GetIntProperty(JsonElement source, string propertyName, int defaultValue = 0)
            => source.TryGetProperty(propertyName, out var prop) ? prop.GetInt32() : defaultValue;

        private string? GetStringProperty(JsonElement source, string propertyName)
            => source.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;

        private decimal GetDecimalProperty(JsonElement source, string propertyName, decimal defaultValue = 0m)
            => source.TryGetProperty(propertyName, out var prop) ? prop.GetDecimal() : defaultValue;

        // Parseo seguro de fechas
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