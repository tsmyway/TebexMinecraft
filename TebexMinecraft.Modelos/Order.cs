using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TebexMinecraft.Modelos;

[Table("orders")]
public class Order
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [StringLength(32)]
    [Column("order_code")]
    public string? OrderCode { get; set; } = string.Empty;

    [Required]
    [Column("user_id")]
    public long UserId { get; set; }

    [Required]
    [Column("total_amount", TypeName = "numeric(10,2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(128)]
    [Column("gateway_payment_id")]
    public string? GatewayPaymentId { get; set; }

    [Required]
    [StringLength(32)]
    [Column("status")]
    public string? Status { get; set; } = "PENDING";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [ForeignKey("UserId")]
    public User? User { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}