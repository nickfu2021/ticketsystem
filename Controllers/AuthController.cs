using Microsoft.AspNetCore.Mvc;
using BandHub.AuthService.ApiSupport;       // 內含 this.ToHttpResult(...) 擴充方法
using BandHub.AuthService.Common;          // ServiceResult<T>
using BandHub.AuthService.Dtos;            // LoginResultDto / TokenDto
using BandHub.AuthService.Services.Auth;

namespace BandHub.AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService service) : ControllerBase
    {
        private readonly IAuthService _service = service;

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
                return this.ToHttpResult(ServiceResult<TokenDto>.Fail("missing_refresh_token"), StatusCodes.Status401Unauthorized);
            }

            // 2) 呼叫服務
            var (result, newRtPlain) = await _service.RefreshAsync(rtPlain, ip, ua);

            // 3) 設定新 RT Cookie（僅成功時）
            if (result.Success && !string.IsNullOrWhiteSpace(newRtPlain))
            {
                Response.Cookies.Append("rt", newRtPlain, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(14)
                });
            }

            // 4) 使用 ToHttpResult 統一輸出
            return this.ToHttpResult(result);
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

    }
}
