using Microsoft.AspNetCore.Mvc;
using BandHub.AuthService.Common;

namespace BandHub.AuthService.ApiSupport;

public static class ServiceResultHttpExtensions
{
    /// <summary>
    /// 將 ServiceResult<T> 轉成 IActionResult，統一狀態碼與輸出格式
    /// </summary>
    public static IActionResult ToHttpResult<T>(this ControllerBase c, ServiceResult<T> result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.Success)
        {
            // 200/201 都可由呼叫端決定
            return new ObjectResult(result) { StatusCode = successStatusCode };
        }

        // 根據錯誤情境選擇適當狀態碼（可再細化策略）
        var status = MapStatusCode(result);
        return new ObjectResult(result) { StatusCode = status };
    }

    /// <summary>
    /// 你可以依情境調整對應（示範用 errors 的 Field/Message 或 ErrorMessage 關鍵字判斷）
    /// </summary>
    private static int MapStatusCode<T>(ServiceResult<T> result)
    {
        var msg = result.ErrorMessage ?? string.Empty;

        // 例：驗證錯
        if ((result.Errors?.Count ?? 0) > 0) return StatusCodes.Status400BadRequest;

        // 例：未授權／憑證錯
        if (msg.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("invalid credentials", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("未授權", StringComparison.OrdinalIgnoreCase))
            return StatusCodes.Status401Unauthorized;

        // 例：找不到
        if (msg.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("找不到", StringComparison.OrdinalIgnoreCase))
            return StatusCodes.Status404NotFound;

        // 例：資源衝突（重複 Email 等）
        if (msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("已存在", StringComparison.OrdinalIgnoreCase))
            return StatusCodes.Status409Conflict;

        // 其他當 400
        return StatusCodes.Status400BadRequest;
    }
}
