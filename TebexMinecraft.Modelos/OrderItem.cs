using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TebexMinecraft.Modelos;

[Table("order_items")]
public class OrderItem
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("order_id")]
    public long OrderId { get; set; }

    [Required]
    [StringLength(32)]
    [Column("product_category")]
    public string ProductCategory { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    [Column("product_internal_name")]
    public string ProductInternalName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Column("unit_price", TypeName = "numeric(10,2)")]
    public decimal UnitPrice { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; } = 1;

    [Column("custom_input")]
    public string? CustomInput { get; set; }

    [ForeignKey("OrderId")]
    [JsonIgnore]
    public Order? Order { get; set; }
}