using Newtonsoft.Json;

namespace PRN232.Lab1.API.Common;

public class ApiResponse<T>
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("data")]
    public T? Data { get; set; }

    [JsonProperty("errors")]
    public object? Errors { get; set; }

    [JsonProperty("pagination", NullValueHandling = NullValueHandling.Ignore)]
    public PaginationMeta? Pagination { get; set; }

    [JsonProperty("_links", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, HalLink>? Links { get; set; }
}

public class PaginationMeta
{
    [JsonProperty("page")] public int Page { get; set; }
    [JsonProperty("pageSize")] public int PageSize { get; set; }
    [JsonProperty("totalItems")] public int TotalItems { get; set; }
    [JsonProperty("totalPages")] public int TotalPages { get; set; }
}

public static class ApiResponseFactory
{
    public static ApiResponse<T> Ok<T>(T data, string msg = "Request processed successfully")
        => new() { Success = true, Message = msg, Data = data };

    public static ApiResponse<T> Created<T>(T data, string msg = "Resource created")
        => new() { Success = true, Message = msg, Data = data };

    public static ApiResponse<object?> Error(string msg, object? errors = null)
        => new() { Success = false, Message = msg, Data = null, Errors = errors };
}
