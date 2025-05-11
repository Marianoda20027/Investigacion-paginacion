using Elasticsearch.Net;
using PaginationApp.Core.Exceptions;
using PaginationApp.Services.ElasticSearch.Contracts;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace PaginationApp.Services.ElasticSearch
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly ElasticConnection _connection;

        public ElasticSearchService(ElasticConnection connection)
        {
            _connection = connection;
        }

        // Método principal que ejecuta la búsqueda en Elasticsearch
        public async Task<string> SearchPartsAsync(Dictionary<string, string> filters, int pageNumber, int pageSize)
        {
            ValidatePagination(pageNumber, pageSize);

            var mustClauses = BuildFilterClauses(filters);
            var query = BuildQuery(mustClauses, pageNumber, pageSize);

            var response = await _connection.Client.SearchAsync<StringResponse>(
                "parts", PostData.Serializable(query));

            if (!response.Success)
                throw new ElasticsearchException("Error al realizar la búsqueda");

            return response.Body;
        }

        // Valida los parámetros de paginación
        private void ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new BadRequestException("Número de página inválido (debe ser ≥ 1)");

            if (pageSize < 1 || pageSize > 301)
                throw new BadRequestException("Tamaño de página inválido (debe ser 1-300)");
        }

        // Construye las cláusulas de filtro para la consulta
        private List<object> BuildFilterClauses(Dictionary<string, string> filters)
        {
            var mustClauses = new List<object>();

            if (filters == null || filters.Count == 0)
            {
                mustClauses.Add(new { match_all = new { } });
                return mustClauses;
            }

            foreach (var filter in filters)
            {
                mustClauses.Add(BuildClause(filter.Key.ToLower(), filter.Value));
            }

            return mustClauses;
        }

        // Construye una cláusula individual según el tipo de filtro
        private object BuildClause(string key, string value)
        {
            return key switch
            {
                // Filtros de rango para fechas
                "productiondate_gte" or "productiondate_lte"
                or "lastmodified_gte" or "lastmodified_lte" => new
                {
                    range = new Dictionary<string, object>
                    {
                        [key.Split('_')[0]] = new Dictionary<string, object>
                        {
                            [key.EndsWith("gte") ? "gte" : "lte"] = value
                        }
                    }
                },

                // Filtros de rango numérico para stock
                "stockquantity_gte" or "stockquantity_lte" => BuildNumericRangeClause("stockquantity", value, key),

                // Filtros de rango decimal para peso
                "unitweight_gte" or "unitweight_lte" => BuildDecimalRangeClause("unitweight", value, key),

                // Búsqueda exacta para partcode (usa campo keyword)
                "partcode" => new 
                {
                    term = new Dictionary<string, object>
                    {
                        ["partcode.keyword"] = value
                    }
                },

                // Búsqueda por texto para categoría
                "category" => new
                {
                    match = new Dictionary<string, object> { [key] = value }
                },

                // Búsqueda difusa para especificaciones técnicas
                "technicalspecs" => new
                {
                    match = new Dictionary<string, object>
                    {
                        ["technicalspecs"] = new { query = value, fuzziness = "AUTO" }
                    }
                },

                _ => null
            };
        }

        // Construye cláusula de rango para valores enteros
        private object BuildNumericRangeClause(string field, string value, string key)
        {
            if (!int.TryParse(value, out int parsedValue))
                throw new BadRequestException($"{key} debe ser un número entero");

            return new
            {
                range = new Dictionary<string, object>
                {
                    [field] = new Dictionary<string, object>
                    {
                        [key.EndsWith("gte") ? "gte" : "lte"] = parsedValue
                    }
                }
            };
        }

        // Construye cláusula de rango para valores decimales
        private object BuildDecimalRangeClause(string field, string value, string key)
        {
            if (!decimal.TryParse(value, out decimal parsedValue))
                throw new BadRequestException($"{key} debe ser un número decimal");

            return new
            {
                range = new Dictionary<string, object>
                {
                    [field] = new Dictionary<string, object>
                    {
                        [key.EndsWith("gte") ? "gte" : "lte"] = parsedValue
                    }
                }
            };
        }

        // Construye la consulta final con paginación
        private object BuildQuery(List<object> mustClauses, int pageNumber, int pageSize)
        {
            return new
            {
                track_total_hits = true,
                query = new { @bool = new { must = mustClauses } },
                from = (pageNumber - 1) * pageSize,
                size = pageSize,
                _source = new[]
                {
                    "id", "partcode", "category", "stockquantity",
                    "unitweight", "productiondate", "lastmodified", "technicalspecs"
                }
            };
        }
    }
}