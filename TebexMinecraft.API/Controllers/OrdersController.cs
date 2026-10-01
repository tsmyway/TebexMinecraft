using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TebexMinecraft.API.Data;
using TebexMinecraft.Modelos;

namespace TebexMinecraft.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly TebexMinecraftAPIContext _context;

        public OrdersController(TebexMinecraftAPIContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(long id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return order;
        }

        // GET: api/Orders/by-code/ORD-XXXX
        [HttpGet("by-code/{orderCode}")]
        public async Task<ActionResult<Order>> GetOrderByCode(string orderCode)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order == null) return NotFound();
            return order;
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder([FromBody] Order order)
        {
            if (order.Items == null || !order.Items.Any())
            {
                return BadRequest("La orden no contiene productos.");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == order.UserId);
            if (!userExists)
            {
                return BadRequest($"El usuario con ID {order.UserId} no existe.");
            }

            order.OrderCode = $"ORD-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            order.CreatedAt = DateTime.UtcNow;
            order.Status = "PENDING";
            order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);
            
            order.User = null;
            foreach (var item in order.Items)
            {
                item.Id = 0;
                item.Order = null;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        // POST: api/Orders/simulate-payment/{orderCode}
        [HttpPost("simulate-payment/{orderCode}")]
        public async Task<IActionResult> SimulatePayment(string orderCode)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order == null) return NotFound("Orden no encontrada.");
            if (order.Status == "PAID") return BadRequest("La orden ya se encuentra pagada.");

            var uuid = order.User?.Uuid;
            if (string.IsNullOrEmpty(uuid))
            {
                return BadRequest("No se pudo obtener el UUID del jugador asociado a la orden.");
            }
            
            foreach (var item in order.Items)
            {
                switch (item.ProductCategory.ToUpper())
                {
                    case "RANK":
                        bool alreadyHasRank = await _context.UserRanks
                            .AnyAsync(r => r.Uuid == uuid && r.ServerRank == item.ProductInternalName);

                        if (!alreadyHasRank)
                        {
                            _context.UserRanks.Add(new UserRank
                            {
                                Uuid = uuid,
                                ServerRank = item.ProductInternalName,
                                AcquiredAt = DateTime.UtcNow
                            });
                        }
                        break;

                    case "PREFIX":
                        bool alreadyHasPrefix = await _context.UserPrefixes
                            .AnyAsync(p => p.Uuid == uuid && p.ServerTag == item.ProductInternalName);

                        if (!alreadyHasPrefix)
                        {
                            _context.UserPrefixes.Add(new UserPrefix
                            {
                                Uuid = uuid,
                                ServerTag = item.ProductInternalName,
                                AcquiredAt = DateTime.UtcNow
                            });
                        }
                        break;

                    case "CONSUMABLE":
                        _context.UserClaims.Add(new UserClaim
                        {
                            Uuid = uuid,
                            ServerItem = item.ProductInternalName,
                            Quantity = item.Quantity,
                            IsClaimed = false
                        });
                        break;

                    case "APPEAL":
                        _context.UserAppeals.Add(new UserAppeal
                        {
                            Uuid = uuid,
                            ServerAppeal = item.ProductInternalName,
                            Reason = item.CustomInput,
                            IsProcessed = false
                        });
                        break;
                }
            }

            order.Status = "PAID";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Orden pagada con éxito y recompensas enviadas al servidor de Minecraft.",
                orderCode = order.OrderCode,
                status = order.Status
            });
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(long id, Order order)
        {
            if (id != order.Id) return BadRequest();

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Orders.Any(e => e.Id == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(long id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}