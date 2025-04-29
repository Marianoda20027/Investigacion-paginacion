using Elasticsearch.Net;
using PaginationApp.Core.Exceptions;
using PaginationApp.Services.ElasticSearch.Contracts;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace PaginationApp.Services.ElasticSearch
{
    // Servicio encargado de construir y ejecutar búsquedas paginadas en Elasticsearch
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly ElasticConnection _connection;

        public ElasticSearchService(ElasticConnection connection)
        {
            _connection = connection;
        }

        // Punto de entrada principal: ejecuta una búsqueda con filtros y paginación
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

        // Construye dinámicamente los filtros de la búsqueda
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

        // Crea un filtro individual según el tipo (rango, texto, fuzzy)
        private object BuildClause(string key, string value)
        {
            return key switch
            {
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

                "stockquantity_gte" or "stockquantity_lte" => BuildNumericRangeClause("stockquantity", value, key),

                "unitweight_gte" or "unitweight_lte" => BuildDecimalRangeClause("unitweight", value, key),

                "partcode" or "category" => new
                {
                    match = new Dictionary<string, object> { [key] = value }
                },

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

        // Construye un filtro por rango numérico entero
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

        // Construye un filtro por rango numérico decimal
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

        // Construye la estructura final de la consulta con filtros y paginación
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
