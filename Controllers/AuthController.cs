using Microsoft.AspNetCore.Mvc;
using TicketSystemApi.Dtos;
using TicketSystemApi.Services.Auth;

namespace TicketSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService service) : ControllerBase
    {
        private readonly IAuthService _service = service;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var ua = Request.Headers.UserAgent.ToString();

            var (result, rt) = await _service.LoginAsync(dto.Email, dto.Password, ip, ua);

            if (!result.Success)
                return Unauthorized(result);

            if (!string.IsNullOrWhiteSpace(rt))
            {
                Response.Cookies.Append("rt", rt, new CookieOptions
                {
                    HttpOnly = true,    //禁止 JavaScript 存取（避免 XSS）
                    Secure = true,  //只在 HTTPS 傳輸
                    SameSite = SameSiteMode.Strict, //限定同網域才送出，防止 CSRF
                    Expires = DateTimeOffset.UtcNow.AddDays(14)
                });
            }

            return Ok(result);
        }


        [HttpPost("register")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)] //Swagger 註解（加強開發體驗）
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _service.RegisterAsync(dto);
            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result);
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            // 若 RT 放在 Cookie
            var rt = Request.Cookies["rt"];
            if (string.IsNullOrWhiteSpace(rt)) return Unauthorized();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers.UserAgent.ToString();

            var res = await _service.RefreshAsync(rt, ip, ua);
            if (!res.Ok) return Unauthorized(new { error = res.Error });

            // 更新 Cookie
            Response.Cookies.Append("rt", res.NewRefreshToken!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(14)
            });

            return Ok(new { access_token = res.AccessToken });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var rt = Request.Cookies["rt"];
            if (string.IsNullOrWhiteSpace(rt)) return Ok();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _service.LogoutAsync(rt, ip);

            Response.Cookies.Delete("rt");
            return Ok();
        }
    }
}
