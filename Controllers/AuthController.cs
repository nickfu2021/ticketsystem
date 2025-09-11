using Microsoft.AspNetCore.Mvc;
using TicketSystemApi.ApiSupport;       // 內含 this.ToHttpResult(...) 擴充方法
using TicketSystemApi.Common;          // ServiceResult<T>
using TicketSystemApi.Dtos;            // LoginResultDto / TokenDto
using TicketSystemApi.Services.Auth;

namespace TicketSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService service) : ControllerBase
    {
        private readonly IAuthService _service = service;

        [HttpPost("login")]
        [ProducesResponseType(typeof(ServiceResult<LoginResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var ua = Request.Headers.UserAgent.ToString();

            var (result, rt) = await _service.LoginAsync(dto.Email, dto.Password, ip, ua);

            // 設置 RefreshToken Cookie（僅在登入成功且有值時）
            if (result.Success && !string.IsNullOrWhiteSpace(rt))
            {
                Response.Cookies.Append("rt", rt, new CookieOptions
                {
                    HttpOnly = true,                 // 禁止 JS 讀取
                    Secure = true,                   // 僅 HTTPS
                    SameSite = SameSiteMode.Strict,  // 防 CSRF
                    Expires = DateTimeOffset.UtcNow.AddDays(14)
                });
            }

            // 統一輸出（成功 200；失敗交由 ToHttpResult 的 Map 決定）
            return this.ToHttpResult(result);
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ServiceResult<Unit>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ServiceResult<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers.UserAgent.ToString();

            var result = await _service.RegisterAsync(dto, ip, ua);
            // 註冊成功用 201 Created（ToHttpResult 第二參數指定成功狀態碼）
            return this.ToHttpResult(result, StatusCodes.Status201Created);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ServiceResult<TokenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult<TokenDto>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers.UserAgent.ToString();

            // 1) 從 Cookie 取出 RT
            var rtPlain = Request.Cookies["rt"];
            if (string.IsNullOrWhiteSpace(rtPlain))
            {
                // 缺少 RT → 401 + 統一包裝
                return Unauthorized(ServiceResult<TokenDto>.Fail("未授權")); // 或 "missing_refresh_token"
            }

            // 2) 呼叫服務
            var (result, newRtPlain) = await _service.RefreshAsync(rtPlain, ip, ua);

            // 3) 若失敗，直接 401（避免 MapStatusCode 把某些訊息判成 400）
            if (!result.Success)
                return Unauthorized(result);

            // 4) 成功則旋轉新 RT Cookie（如有）
            if (!string.IsNullOrWhiteSpace(newRtPlain))
            {
                Response.Cookies.Append("rt", newRtPlain, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(14)
                });
            }

            // 5) 成功回 200 帶 accessToken/expiresIn
            return Ok(result);
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            var rt = Request.Cookies["rt"];
            if (!string.IsNullOrWhiteSpace(rt))
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _service.LogoutAsync(rt, ip);
                Response.Cookies.Delete("rt");
            }
            return Ok();
        }

        [HttpGet("verify-email")]
        [ProducesResponseType(typeof(ServiceResult<Unit>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServiceResult<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var result = await _service.VerifyAsync(token);
            // 缺 token → 400，其餘失敗（過期/無效）→ 401
            var okStatus = StatusCodes.Status200OK;
            if (!result.Success && result.ErrorMessage == "missing_token")
                return this.ToHttpResult(result, StatusCodes.Status400BadRequest);

            return this.ToHttpResult(result, okStatus);
        }

    }
}
