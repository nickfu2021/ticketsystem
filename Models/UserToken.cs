using System;
using System.Net;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TicketSystemApi.Models;


[Table("user_tokens")]
public class UserToken
{
    [Key]
    [Column("token_uuid")]
    public Guid TokenUuid { get; set; } = Guid.NewGuid();

    [Column("user_uuid")]
    public Guid UserUuid { get; set; }

    [Column("purpose")]
    public string Purpose { get; set; } = "email_verify";  

    [Column("token_hash")]
    public byte[] TokenHash { get; set; } = [];

    [Column("sent_to")]
    public string? SentTo { get; set; }

    [Column("new_email")]
    public string? NewEmail { get; set; }

    [Column("created_at", TypeName = "timestamptz")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("expires_at", TypeName = "timestamptz")]
    public DateTime ExpiresAt { get; set; }

    [Column("consumed_at", TypeName = "timestamptz")]
    public DateTime? ConsumedAt { get; set; }

    [Column("revoked_at", TypeName = "timestamptz")]
    public DateTime? RevokedAt { get; set; }

    [Column("ip_created")]
    public string? IpCreated { get; set; }

    [Column("ua_created")]
    public string? UaCreated { get; set; }

    [Column("meta", TypeName = "jsonb")]
    public string Meta { get; set; } = "{}";

}
