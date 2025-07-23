using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TicketSystemApi.Common;

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
        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        // 攔截 FluentValidation 的 ModelState 錯誤（400 BadRequest）
        if (context.Response.StatusCode == 400 && !context.Response.HasStarted)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            var originalResponse = await new StreamReader(memoryStream).ReadToEndAsync();

            if (originalResponse.Contains("\"errors\""))
            {
                var errors = JsonSerializer.Deserialize<ValidationProblemDetails>(originalResponse);

                if (errors?.Errors != null)
                {
                    var fieldErrors = errors.Errors
                        .SelectMany(kvp => kvp.Value.Select(msg => new FieldError
                        {
                            Field = kvp.Key,
                            Message = msg
                        }))
                        .ToList();

                    var resultObj = ServiceResult<object>.Fail("欄位驗證錯誤", fieldErrors);

                    context.Response.ContentType = "application/json";
                    context.Response.Body = originalBodyStream;
                    context.Response.StatusCode = 400;

                    await context.Response.WriteAsJsonAsync(resultObj);
                    return;
                }
            }
        }

        // 回傳原始內容
        memoryStream.Seek(0, SeekOrigin.Begin);
        await memoryStream.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;
    }
}
