namespace TicketSystemApi.Common;

public class ServiceResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<FieldError>? Errors { get; set; }
    /*
        理由                說明
        結構一致            所有 API 回傳格式相同，前端 .data 不會因 success 為 false 而變成 undefined
        防止解構錯誤        避免前端寫 res.data.data.xxx 時，發生 TypeError: Cannot read property 'xxx' of undefined
        更易於包裝 wrapper	前端如 axios interceptors、fetch helper 可以完全統一格式處理
    */
    public object? Data { get; set; } = null; // 統一保留 data 欄位

    public static ServiceResult Ok() => new()
    {
        Success = true,
        Data = null
    };

    public static ServiceResult Fail(string message) => new()
    {
        Success = false,
        ErrorMessage = message,
        Data = null
    };

    public static ServiceResult Fail(string message, List<FieldError> errors) => new()
    {
        Success = false,
        ErrorMessage = message,
        Errors = errors,
        Data = null
    };
}

public class ServiceResult<T> : ServiceResult
{
    public new T? Data { get; set; }

    public static ServiceResult<T> Ok(T data) => new()
    {
        Success = true,
        Data = data,
    };

    public static new ServiceResult<T> Fail(string message) => new()
    {
        Success = false,
        ErrorMessage = message,
        Data = default
    };

    public static new ServiceResult<T> Fail(string message, List<FieldError> errors) => new()
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