using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSystemApi.Models;

[Table("users")]
public class User
{
    // 不需要在資料類別模型寫 [Required], [StringLength], [Range], [EmailAddress]，統一由 __DtoValidator.cs 做驗證就好
    // 如果不用 EF Migration，就不用在 Entity 上寫 [StringLength]、[Required] 等 DataAnnotation，只保留資料庫映射需要的欄位資訊即可，反之則要。

    [Key]
    [Column("user_uuid")]
    public Guid UserUuid { get; set; } = Guid.NewGuid();
    [Column("email")]
    public string Email { get; set; } = string.Empty;
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;
    [Column("username")]
    public string Username { get; set; } = string.Empty;
    [Column("id_number")]
    public string? IdNumber { get; set; }
    [Column("birthday")]
    public string? Birthday { get; set; }
    [Column("mobile_number")]
    public string MobileNumber { get; set; } = string.Empty;
    [Column("postal_code")]
    public string? PostalCode { get; set; }
    [Column("address")]
    public string? Address { get; set; }
    [Column("is_active")]
    public bool IsActive { get; set; } = false;
    [Column("is_locked")]
    public bool IsLocked { get; set; } = false;
    //TypeName = "timestamptz" 告訴 EF：「請把這個 DateTime 欄位當成 PostgreSQL 的 timestamp with time zone」，以正確儲存和處理 UTC 時間。
    [Column("created_at", TypeName = "timestamptz")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at", TypeName = "timestamptz")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [Column("email_verified_at", TypeName = "timestamptz")]
    public DateTime? EmailVerifiedAt { get; set; }
    [Column("last_login_at", TypeName = "timestamptz")]
    public DateTime? LastLoginAt { get; set; }
}
