using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TebexMinecraft.Modelos;

[Table("user_claims")]
public class UserClaim
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
    [Column("server_item")]
    public string ServerItem { get; set; } = string.Empty;

    [Column("quantity")]
    public int Quantity { get; set; } = 1;

    [Column("is_claimed")]
    public bool IsClaimed { get; set; } = false;

    [Column("claimed_at")]
    public DateTime? ClaimedAt { get; set; }
}