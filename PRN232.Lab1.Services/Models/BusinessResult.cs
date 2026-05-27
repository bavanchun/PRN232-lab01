namespace PRN232.Lab1.Services.Models;

public class BusinessResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static BusinessResult<T> Ok(T data, string message = "Operation succeeded") =>
        new() { Success = true, Message = message, Data = data };

    public static BusinessResult<T> Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };

    public static BusinessResult<T> NotFound(string message = "Resource not found") =>
        new() { Success = false, Message = message };
}
