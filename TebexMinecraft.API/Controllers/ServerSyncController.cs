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
    public class ServerSyncController : ControllerBase
    {
        private readonly TebexMinecraftAPIContext _context;

        public ServerSyncController(TebexMinecraftAPIContext context)
        {
            _context = context;
        }

        // GET: api/ServerSync/refresh/{uuid}
        [HttpGet("refresh/{uuid}")]
        public async Task<IActionResult> Refresh(string uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid))
            {
                return BadRequest("El UUID no puede estar vacío.");
            }
            
            List<UserRank> ranks = await _context.UserRanks
                .AsNoTracking()
                .Where(r => r.Uuid == uuid)
                .ToListAsync();
            
            List<UserPrefix> prefixes = await _context.UserPrefixes
                .AsNoTracking()
                .Where(p => p.Uuid == uuid)
                .ToListAsync();
            
            List<UserClaim> claims = await _context.UserClaims
                .AsNoTracking()
                .Where(c => c.Uuid == uuid && !c.IsClaimed)
                .ToListAsync();
            
            List<UserAppeal> appeals = await _context.UserAppeals
                .AsNoTracking()
                .Where(a => a.Uuid == uuid && !a.IsProcessed)
                .ToListAsync();

            return Ok(new
            {
                uuid = uuid,
                ranks = ranks,
                prefixes = prefixes,
                claims = claims,
                appeals = appeals
            });
        }

        // POST: api/ServerSync/claim/{id}
        [HttpPost("claim/{id}")]
        public async Task<IActionResult> ConfirmClaim(long id)
        {
            var claim = await _context.UserClaims.FindAsync(id);

            if (claim == null)
            {
                return NotFound($"Reclamo con ID {id} no encontrado.");
            }

            if (claim.IsClaimed)
            {
                return BadRequest("Este reclamo ya fue entregado previamente.");
            }

            claim.IsClaimed = true;
            claim.ClaimedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(claim);
        }

        // POST: api/ServerSync/process-appeal/{id}
        [HttpPost("process-appeal/{id}")]
        public async Task<IActionResult> ConfirmAppeal(long id)
        {
            var appeal = await _context.UserAppeals.FindAsync(id);

            if (appeal == null)
            {
                return NotFound($"Apelación con ID {id} no encontrada.");
            }

            if (appeal.IsProcessed)
            {
                return BadRequest("Esta apelación ya fue procesada previamente.");
            }

            appeal.IsProcessed = true;
            appeal.ProcessedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(appeal);
        }
    }
}