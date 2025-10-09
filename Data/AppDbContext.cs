using Microsoft.EntityFrameworkCore;
using BandHub.AuthService.Models;

namespace BandHub.AuthService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> option) : DbContext(option)
{
    //（Expression Body, C# 6+）
    // 這是 唯讀屬性，EF Core 執行 Set<T>() 來取出 DbSet<T>。
    // 沒有 set;，所以你不能在程式裡手動指定 Users = ...，但 EF Core 在初始化時會自己處理。
    // 這種寫法簡潔，常見於 code-first 新專案。
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    // 傳統屬性寫法
    // 有 get; set; → EF Core 在啟動時會自動注入 DbSet<T> 實例。
    // 也允許你在程式碼中測試時 mock（自己 new 一個 DbSet 代替）。
    // 這是 EF Core 文件範例裡最常見的寫法。
    public DbSet<Postal> Postal { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }
}
