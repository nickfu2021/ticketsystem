using System.IdentityModel;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TicketSystemApi.Common;
using System.IdentityModel.Tokens.Jwt;
using TicketSystemApi.Configurations;

namespace TicketSystemApi.Services.Auth;


public class TokenService(IOptions<JwtSettings> jwt) : ITokenService
{
    private readonly JwtSettings _jwt = jwt.Value;

    public string CreateToken(string userId, IEnumerable<string>? roles = null)
    {
        // 1. 定義 Claims（使用者身分資訊）
        var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),   // Sub 是 JWT 的標準欄位，用於跨語言的身分識別
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),    // Token 唯一 ID
                new(ClaimTypes.NameIdentifier, userId)  // ClaimTypes.NameIdentifier 是 .NET Identity 系統內部欄位，用於 User.Identity 與授權框架。
                
                //Sub 與 ClaimTypes.NameIdentifier 它們都存放 userId，只是用不同命名規範，讓系統能同時支援 JWT 標準與 ASP.NET 的 Claims API。
            };

        if (roles != null)
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        // 2. 建立簽章金鑰（用 appsettings.json 的 SecretKey）
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 3. 建立 JWT Token 物件
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiresMinutes),
            signingCredentials: creds);

        // 4. 轉成字串回傳給前端
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
