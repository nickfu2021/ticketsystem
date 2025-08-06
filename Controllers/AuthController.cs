using Microsoft.AspNetCore.Mvc;
using TicketSystemApi.Services.Auth;
using TicketSystemApi.Dtos;
using TicketSystemApi.Common;

namespace TicketSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(JwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost]
    public IActionResult Login([FromBody] LoginDto dto)
    {
          if (dto.Username == "admin" && dto.Password == "123456")
            {
                var token = _jwtTokenService.GenerateToken(userId: "1", role: "Admin");

                var result = ServiceResult<string>.Ok(token);
                return Ok(result);
            }

            var failResult = ServiceResult<string>.Fail("帳號或密碼錯誤");
            return Unauthorized(failResult);
    }
}