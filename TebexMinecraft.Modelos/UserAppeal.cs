using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TebexMinecraft.Modelos;

[Table("user_appeals")]
public class UserAppeal
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
    [Column("server_appeal")]
    public string ServerAppeal { get; set; } = string.Empty;

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("is_processed")]
    public bool IsProcessed { get; set; } = false;

    [Column("processed_at")]
    public DateTime? ProcessedAt { get; set; }
}