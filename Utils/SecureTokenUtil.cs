using System.Security.Cryptography;
using System.Text;

namespace TicketSystemApi.Utils;

public static class SecureTokenUtil
{
    // 高熵 token（給使用者，放在 URL 裡）
    public static string GenerateToken(int size = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(size);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_'); // base64url
    }

    // 雜湊：用於 DB 儲存（使用 SHA-256 + pepper）
    public static byte[] Hash(string tokenPlain, string pepper)
    {
        var input = Encoding.UTF8.GetBytes(tokenPlain + pepper);
        return SHA256.HashData(input); // 回傳 byte[]
    }

    // 檢查雜湊是否符合（常用於驗證點擊信）
    public static bool Verify(string tokenPlain, string pepper, byte[] hash)
    {
        var input = Encoding.UTF8.GetBytes(tokenPlain + pepper);
        var inputHash = SHA256.HashData(input);
        return CryptographicOperations.FixedTimeEquals(inputHash, hash);
    }
}
