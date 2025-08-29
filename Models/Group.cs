using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSystemApi.Models;

[Table("groups")]
public class Group
{
    [Key]
    [Column("group_uuid")]
    public Guid GroupUuid { get; set; } = Guid.NewGuid();
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("description")]
    public string Description { get; set; } = string.Empty;
    //TypeName = "timestamptz" 告訴 EF：「請把這個 DateTime 欄位當成 PostgreSQL 的 timestamp with time zone」，以正確儲存和處理 UTC 時間。
    [Column("created_at", TypeName = "timestamptz")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at", TypeName = "timestamptz")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}