using PaginationApp.Core.Exceptions;
using System;

namespace PaginationApp.Core.Utilities.Validators
{
    public static class PartValidator
    {
        public static void ValidateSearchParams(
            int pageNumber,
            int pageSize,
            int? minStockQuantity = null,
            int? maxStockQuantity = null,
            decimal? minUnitWeight = null,
            decimal? maxUnitWeight = null,
            string? productionDateStart = null,
            string? productionDateEnd = null,
            string? lastModifiedStart = null,
            string? lastModifiedEnd = null)
        {
            ValidatePagination(pageNumber, pageSize);
            ValidateRanges(minStockQuantity, maxStockQuantity, minUnitWeight, maxUnitWeight);
            ValidateDateRanges(productionDateStart, productionDateEnd, lastModifiedStart, lastModifiedEnd);
        }

        private static void ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new BadRequestException("Page number must be ≥ 1");

            if (pageSize < 1 || pageSize > 300)
                throw new BadRequestException("Page size must be 1-300");
        }

        private static void ValidateRanges(
            int? minStock, int? maxStock,
            decimal? minWeight, decimal? maxWeight)
        {
            ValidateRange(minStock, maxStock, "Stock quantity");
            ValidateRange(minWeight, maxWeight, "Unit weight");
        }

        private static void ValidateRange<T>(T? min, T? max, string fieldName) 
            where T : struct, IComparable<T>
        {
            if (min.HasValue && max.HasValue && min.Value.CompareTo(max.Value) > 0)
                throw new BadRequestException($"{fieldName}: Min cannot be greater than max");
        }

        private static void ValidateDateRanges(
            string? prodStart, string? prodEnd,
            string? modStart, string? modEnd)
        {
            ValidateDateRange(prodStart, prodEnd, "Production date");
            ValidateDateRange(modStart, modEnd, "Last modified date");
        }

        private static void ValidateDateRange(string? start, string? end, string fieldName)
        {
            if (!string.IsNullOrEmpty(start) && !DateTime.TryParse(start, out _))
                throw new BadRequestException($"Invalid {fieldName} start format (use ISO8601)");

            if (!string.IsNullOrEmpty(end) && !DateTime.TryParse(end, out _))
                throw new BadRequestException($"Invalid {fieldName} end format (use ISO8601)");

            if (DateTime.TryParse(start, out var startDate) && 
                DateTime.TryParse(end, out var endDate) && 
                startDate > endDate)
            {
                throw new BadRequestException($"{fieldName}: Start cannot be after end");
            }
        }
    }
}