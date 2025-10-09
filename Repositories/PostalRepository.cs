using BandHub.AuthService.Data;
using Microsoft.EntityFrameworkCore;

namespace BandHub.AuthService.Repositories;

public class PostalRepository(AppDbContext context) : IPostalRepository
{
    private readonly AppDbContext _context = context;

    public async Task<bool> ExistsZipCityDistrictAsync(string zipCode, string city, string district, CancellationToken cancellationToken = default)
    {
        /*
            CancellationToken 是什麼？
            它是用來「中斷非同步作業」的機制。
            在 .NET 中，非同步方法常常接收一個 CancellationToken，目的是讓呼叫端可以「取消」一個還沒完成的任務，例如：
            1.HTTP 請求被使用者中止（關閉瀏覽器、切換頁面）
            2.任務逾時（timeout）
            3.開發者手動取消

            資源效率	停止不必要的查詢（省 DB 連線 & CPU）
            響應性佳	使用者按「取消」或關閉頁面時能快速中止請求
            與 ASP.NET 請求生命週期整合	請求中止，HttpContext.RequestAborted 會自動傳入所有 validator 的 cancellationToken
        */
        return await _context.Postal.AnyAsync(p => p.ZipCode == zipCode
        && p.City == city && p.District == district, cancellationToken);
    }
}
