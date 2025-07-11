using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace TicketSystemApi.Middleware;

public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationExceptionMiddleware(RequestDelegate next)
    {
        _next = next;   // 把 next 想像成 MathOp 但框架會直接幫我指定方法給 next
    }

    public async Task Invoke(HttpContext context)
    {
        // 建立 response 捕捉
        var originalBodyStream = context.Response.Body;

        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context); // 繼續往下執行

        // 假如是 ModelState 錯誤（400）
        if (context.Response.StatusCode == 400 && context.Response.HasStarted == false)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            var originalResponse = await new StreamReader(memoryStream).ReadToEndAsync();

            if (originalResponse.Contains("\"errors\"")) // 粗略判斷是否是 FluentValidation 錯誤
            {
                var errors = JsonSerializer.Deserialize<ValidationProblemDetails>(originalResponse);

                if (errors?.Errors != null)
                {
                    var response = new
                    {
                        success = false,
                        message = "驗證失敗",
                        errors = errors.Errors.SelectMany(kvp => kvp.Value.Select(msg => new
                        {
                            field = kvp.Key,
                            message = msg
                        }))
                    };

                    context.Response.ContentType = "application/json";
                    context.Response.Body = originalBodyStream;
                    await context.Response.WriteAsJsonAsync(response);
                    return;
                }
            }
        }

        // 非驗證錯誤 → 還原原本回傳
        memoryStream.Seek(0, SeekOrigin.Begin);
        await memoryStream.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;
    }
}
