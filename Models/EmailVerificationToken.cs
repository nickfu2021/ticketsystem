using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSystemApi.Models;
[Table("email_verification_tokens")]
public class EmailVerificationToken
{
    [Key]
    [Column("token_id")]
    public Guid TokenId { get; set; }

    [Column("user_uuid")]
    public Guid UserUuid { get; set; }

    [Column("token_hash")]
    public string TokenHash { get; set; } = string.Empty;

    [Column("expires_at", TypeName = "timestamptz")]
    public DateTime ExpiresAt { get; set; }

    [Column("created_at", TypeName = "timestamptz")]
    public DateTime CreatedAt { get; set; }

    [Column("created_ip")]
    public string? CreatedIp { get; set; }

    [Column("created_ua")]
    public string? CreatedUa { get; set; }

    [Column("consumed_at", TypeName = "timestamptz")]
    public DateTime? ConsumedAt { get; set; }
}
