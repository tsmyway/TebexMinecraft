using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TebexMinecraft.Modelos;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [StringLength(36)]
    [Column("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [Required]
    [StringLength(16)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;
}