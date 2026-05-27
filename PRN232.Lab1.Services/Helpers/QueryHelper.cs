using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace PRN232.Lab1.Services.Helpers;

public static class QueryHelper
{
    public static IQueryable<T> ApplySearch<T>(
        IQueryable<T> query,
        string? search,
        params Expression<Func<T, string?>>[] searchableFields)
    {
        if (string.IsNullOrWhiteSpace(search) || searchableFields.Length == 0)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var searchValue = Expression.Constant(search.ToLower(), typeof(string));
        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

        Expression? combined = null;
        foreach (var field in searchableFields)
        {
            var fieldExpr = Expression.Invoke(field, parameter);
            var nullCheck = Expression.Coalesce(fieldExpr, Expression.Constant(string.Empty));
            var toLower = Expression.Call(nullCheck, toLowerMethod);
            var contains = Expression.Call(toLower, containsMethod, searchValue);
            combined = combined == null ? (Expression)contains : Expression.OrElse(combined, contains);
        }

        if (combined == null) return query;
        var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
        return query.Where(lambda);
    }

    public static IQueryable<T> ApplySort<T>(IQueryable<T> query, string? sort, HashSet<string> allowedFields, string defaultSort)
    {
        var spec = string.IsNullOrWhiteSpace(sort) ? defaultSort : sort;
        var parts = spec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var dynamicOrder = new List<string>();
        foreach (var p in parts)
        {
            var desc = p.StartsWith('-');
            var name = desc ? p[1..] : p;
            if (!allowedFields.Contains(name, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Sort field '{name}' is not allowed. Allowed: {string.Join(", ", allowedFields)}");
            var pascal = char.ToUpperInvariant(name[0]) + name[1..];
            dynamicOrder.Add(desc ? $"{pascal} descending" : pascal);
        }

        return dynamicOrder.Count == 0 ? query : query.OrderBy(string.Join(", ", dynamicOrder));
    }

    public static async Task<PRN232.Lab1.Services.Models.PagedResult<TBusiness>> ToPagedAsync<TEntity, TBusiness>(
        IQueryable<TEntity> q, int page, int size, Func<TEntity, TBusiness> map)
    {
        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 200);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * size).Take(size).ToListAsync();
        return new PRN232.Lab1.Services.Models.PagedResult<TBusiness>
        {
            Items = items.Select(map).ToList(),
            Page = page,
            PageSize = size,
            TotalItems = total
        };
    }

    public static bool ShouldExpand(string? expand, string keyword)
    {
        if (string.IsNullOrWhiteSpace(expand)) return false;
        return expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(part => string.Equals(part, keyword, StringComparison.OrdinalIgnoreCase));
    }
}
