using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TebexMinecraft.Modelos;

[Table("user_ranks")]
public class UserRank
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
    [Column("server_rank")]
    public string ServerRank { get; set; } = string.Empty;

    [Column("acquired_at")]
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
}