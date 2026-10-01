using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TebexMinecraft.Modelos;

[Table("ranks")]
public class Rank
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("price", TypeName = "numeric(10,2)")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(32)]
    [Column("server_rank")]
    public string ServerRank { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }
}