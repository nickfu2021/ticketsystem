using System.Security.Cryptography;
using System.Text;

namespace TicketSystemApi.Services.Auth;

public static class RefreshTokenUtil
{
    public static string GeneratePlainToken(int bytes = 64)
    {
        var buf = RandomNumberGenerator.GetBytes(bytes); // 64 bytes -> 86字元Base64
        return Convert.ToBase64String(buf);              // 不用 JWT，純高熵字串
    }

    public static string Hash(string plain, string pepper)
    {
        // Hash(peppper + plain) → 存 DB
        using var sha = SHA256.Create();
        var input = Encoding.UTF8.GetBytes(pepper + plain);
        // var bytes = sha.ComputeHash(input);
        var bytes = SHA256.HashData(input); // 靜態方法，取代 ComputeHash
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
