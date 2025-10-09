using BandHub.AuthService.Common;

namespace BandHub.AuthService.Middleware;

// ApiExceptionMiddleware 的角色
// 它是你的「最後一道保險網」：
// 當請求一路往下走，Controller 或 Service 沒有處理好，真的拋出 Exception 時 → 就會被這個 Middleware 攔到。
// 在這裡你統一把錯誤轉成 JSON 格式（例如 ServiceResult.Fail("系統發生未預期錯誤")）。

// 它會處理哪些？
// 未預期的 Exception → 回 500 Internal Server Error
// 例如：NullReferenceException、資料庫斷線、SMTP 連不上。
// 這些是前端無法預期也無法修正的。
// 你主動丟的某些特殊 Exception → 可以轉成對應狀態碼
// 例如：你在 Service 丟 UnauthorizedAccessException，Middleware 可以把它變成 401。
// 或者你自訂 DomainException，丟出來 → Middleware 把它包成 400。

// 哪些不走 ApiExceptionMiddleware？
// Validator 錯誤（欄位格式錯誤、商業規則失敗） → 不會丟 Exception，本來就會走 ModelState → 直接回 400。
// 你在 Controller 明確回傳 BadRequest()、Unauthorized()、NotFound() → 直接出去，不會進 Middleware。

// 所以可以這樣記：
// 400 → Validator / 商業規則錯誤（「我檢查過，你的輸入不對」）
// 401/403/404 → 你自己在程式邏輯中檢查後，主動回的錯誤
// 500 → 程式真的爆掉、沒處理到的錯 → 這時候才交給 ApiExceptionMiddleware 收尾

public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext ctx)
    {
        var cid = ctx.TraceIdentifier; // correlation id

        try
        {
            await _next(ctx);
        }
        catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
        {
            // 用戶端中止（瀏覽器關頁/取消），不當作伺服器 500
            _logger.LogInformation("Request aborted by client. {Method} {Path} cid={CID}",
                ctx.Request.Method, ctx.Request.Path, cid);
            await TryWriteJson(ctx, StatusCodes.Status499ClientClosedRequest,
                ServiceResult<object?>.Fail("用戶端已中止請求")); // 499 非正式標準，但常見慣例
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized. {Method} {Path} cid={CID}",
                ctx.Request.Method, ctx.Request.Path, cid);
            await TryWriteJson(ctx, StatusCodes.Status401Unauthorized,
                ServiceResult<object?>.Fail("未授權"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception. {Method} {Path} cid={CID}",
                ctx.Request.Method, ctx.Request.Path, cid);
            await TryWriteJson(ctx, StatusCodes.Status500InternalServerError,
                ServiceResult<object?>.Fail("系統發生未預期錯誤"));
        }
    }

    private static async Task TryWriteJson(HttpContext ctx, int status, ServiceResult<object?> body)
    {
        // 回傳前帶上 CorrelationId 讓前端/追蹤更好（若你想把它放進 body，也可擴充 ServiceResult）
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        ctx.Response.Headers["X-Correlation-ID"] = ctx.TraceIdentifier;

        // 若回應已開始寫出，就不要再寫，避免二度例外
        if (ctx.Response.HasStarted)
            return;

        await ctx.Response.WriteAsJsonAsync(body);
    }
}