using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSystemApi.Models;

[Table("refresh_tokens")]
public class RefreshToken
{
    [Key]
    [Column("rt_uuid")]
    public Guid RtUuid { get; set; }

    [Column("user_uuid")]
    public Guid UserUuid { get; set; }

    [Column("token_hash")]
    public string TokenHash { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("created_by_ip")]
    public string? CreatedByIp { get; set; }

    [Column("user_agent")]
    public string? UserAgent { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    [Column("replaced_by")]
    public string? ReplacedBy { get; set; }

    [Column("revoked_reason")]
    public string? RevokedReason { get; set; }

    [Column("last_used_at")]
    public DateTime? LastUsedAt { get; set; }

    [Column("last_used_ip")]
    public string? LastUsedIp { get; set; }

    public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;
}
