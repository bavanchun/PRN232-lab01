using System.Reflection;

namespace PRN232.Lab1.API.Common;

public static class FieldShaper
{
    public static object Shape<T>(T obj, string? fields) where T : class
    {
        if (string.IsNullOrWhiteSpace(fields)) return obj;

        var requested = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(f => f.ToLowerInvariant()).ToHashSet();

        var dict = new Dictionary<string, object?>();
        foreach (var p in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var camel = char.ToLowerInvariant(p.Name[0]) + p.Name[1..];
            var isLinks = string.Equals(p.Name, "Links", StringComparison.OrdinalIgnoreCase);
            if (isLinks || requested.Contains(camel.ToLowerInvariant()) || requested.Contains("_links") && isLinks)
            {
                var key = isLinks ? "_links" : camel;
                dict[key] = p.GetValue(obj);
            }
        }
        return dict;
    }

    public static IEnumerable<object> ShapeMany<T>(IEnumerable<T> items, string? fields) where T : class
        => string.IsNullOrWhiteSpace(fields)
            ? items
            : items.Select(i => Shape(i, fields));
}
