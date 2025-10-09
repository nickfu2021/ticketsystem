namespace BandHub.AuthService.Common;

// 統一格式 : 所有 API 都有 success、errorMessage、data
// 型別安全 : 有回傳型別 T，編譯時期即檢查正確性
public class ServiceResult<T>
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public List<FieldError>? Errors { get; init; }
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data) => new()
    {
        Success = true,
        Data = data
    };

    public static ServiceResult<T> Fail(string message) => new()
    {
        Success = false,
        ErrorMessage = message,
        Data = default
    };

    public static ServiceResult<T> Fail(string message, List<FieldError> errors) => new()
    {
        Success = false,
        ErrorMessage = message,
        Errors = errors,
        Data = default
    };
}

public class FieldError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}