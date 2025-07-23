namespace TicketSystemApi.Common;

public class ServiceResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<FieldError>? Errors { get; set; }

    public static ServiceResult Ok() => new() { Success = true };

    public static ServiceResult Fail(string message) => new() { Success = false, ErrorMessage = message };

    public static ServiceResult Fail(string message, List<FieldError> errors) => new()
    {
        Success = false,
        ErrorMessage = message,
        Errors = errors
    };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data) => new()
    {
        Success = true,
        Data = data,
    };

    public static new ServiceResult<T> Fail(string message) => new()
    {
        Success = false,
        ErrorMessage = message,
    };

    public static new ServiceResult<T> Fail(string message, List<FieldError> errors) => new()
    {
        Success = false,
        ErrorMessage = message,
        Errors = errors
    };
}

public class FieldError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}