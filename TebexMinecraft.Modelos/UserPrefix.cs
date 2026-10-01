using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TebexMinecraft.Modelos;

[Table("user_prefixes")]
public class UserPrefix
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [StringLength(36)]
    [Column("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [Required]
    [StringLength(32)]
    [Column("server_tag")]
    public string ServerTag { get; set; } = string.Empty;

    [Column("acquired_at")]
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
}