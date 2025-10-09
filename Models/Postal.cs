using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BandHub.AuthService.Models;

[Table("postal")]
public class Postal
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("zip_code")]
    public string ZipCode { get; set; } = string.Empty;
    [Column("city")]
    public string City { get; set; } = string.Empty;
    [Column("district")]
    public string District { get; set; } = string.Empty;
    [Column("road")]
    public string Road { get; set; } = string.Empty;
}
