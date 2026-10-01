using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TebexMinecraft.Modelos;

[Table("consumables")]
public class Consumable
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
    [Column("server_item")]
    public string ServerItem { get; set; } = string.Empty;

    [Column("quantity")]
    public int Quantity { get; set; } = 1;
}