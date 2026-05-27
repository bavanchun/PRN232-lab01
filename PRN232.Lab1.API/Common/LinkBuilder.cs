using Microsoft.AspNetCore.Routing;

namespace PRN232.Lab1.API.Common;

public class LinkBuilder
{
    private readonly LinkGenerator _gen;
    private readonly IHttpContextAccessor _http;

    public LinkBuilder(LinkGenerator gen, IHttpContextAccessor http)
    {
        _gen = gen; _http = http;
    }

    public Dictionary<string, HalLink> ForItem(
        string resource,
        object routeValues,
        IEnumerable<(string rel, string method, string route, object vals)>? extras = null)
    {
        var ctx = _http.HttpContext;
        if (ctx is null) return new();
        var rv = MergeWithVersion(routeValues);
        var links = new Dictionary<string, HalLink>
        {
            ["self"]   = new() { Href = _gen.GetUriByName(ctx, $"{resource}.GetById", rv) ?? string.Empty, Method = "GET" },
            ["update"] = new() { Href = _gen.GetUriByName(ctx, $"{resource}.Update",  rv) ?? string.Empty, Method = "PUT" },
            ["patch"]  = new() { Href = _gen.GetUriByName(ctx, $"{resource}.Patch",   rv) ?? string.Empty, Method = "PATCH" },
            ["delete"] = new() { Href = _gen.GetUriByName(ctx, $"{resource}.Delete",  rv) ?? string.Empty, Method = "DELETE" }
        };
        if (extras != null)
        {
            foreach (var (rel, m, route, vals) in extras)
                links[rel] = new HalLink { Href = _gen.GetUriByName(ctx, route, MergeWithVersion(vals)) ?? string.Empty, Method = m };
        }
        return links;
    }

    public Dictionary<string, HalLink> ForCollection(
        string routeName,
        int page, int pageSize, int totalPages,
        IDictionary<string, string?>? extraQs = null)
    {
        var ctx = _http.HttpContext;
        if (ctx is null) return new();

        string Build(int p)
        {
            var qs = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["page"] = p,
                ["size"] = pageSize
            };
            if (extraQs != null)
                foreach (var kv in extraQs)
                    if (!qs.ContainsKey(kv.Key) && kv.Value != null) qs[kv.Key] = kv.Value;
            var rv = MergeWithVersion(qs);
            return _gen.GetUriByName(ctx, routeName, rv) ?? string.Empty;
        }

        return new Dictionary<string, HalLink>
        {
            ["self"]  = new() { Href = Build(page), Method = "GET" },
            ["first"] = new() { Href = Build(1), Method = "GET" },
            ["prev"]  = new() { Href = page > 1 ? Build(page - 1) : string.Empty, Method = "GET" },
            ["next"]  = new() { Href = page < totalPages ? Build(page + 1) : string.Empty, Method = "GET" },
            ["last"]  = new() { Href = Build(Math.Max(totalPages, 1)), Method = "GET" }
        };
    }

    private static RouteValueDictionary MergeWithVersion(object routeValues)
    {
        var dict = new RouteValueDictionary(routeValues);
        if (!dict.ContainsKey("version")) dict["version"] = "1.0";
        return dict;
    }
}
