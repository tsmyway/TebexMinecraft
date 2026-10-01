using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TebexMinecraft.API.Data;
using TebexMinecraft.Modelos;

namespace TebexMinecraft.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrefixesController : ControllerBase
    {
        private readonly TebexMinecraftAPIContext _context;

        public PrefixesController(TebexMinecraftAPIContext context)
        {
            _context = context;
        }

        // GET: api/Prefixes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prefix>>> GetPrefixes()
        {
            return await _context.Prefixes.ToListAsync();
        }

        // GET: api/Prefixes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Prefix>> GetPrefix(int id)
        {
            var prefix = await _context.Prefixes.FindAsync(id);

            if (prefix == null)
            {
                return NotFound();
            }

            return prefix;
        }

        // PUT: api/Prefixes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPrefix(int id, Prefix prefix)
        {
            if (id != prefix.Id)
            {
                return BadRequest();
            }

            _context.Entry(prefix).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PrefixExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Prefixes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Prefix>> PostPrefix(Prefix prefix)
        {
            _context.Prefixes.Add(prefix);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPrefix", new { id = prefix.Id }, prefix);
        }

        // DELETE: api/Prefixes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrefix(int id)
        {
            var prefix = await _context.Prefixes.FindAsync(id);
            if (prefix == null)
            {
                return NotFound();
            }

            _context.Prefixes.Remove(prefix);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PrefixExists(int id)
        {
            return _context.Prefixes.Any(e => e.Id == id);
        }
    }
}
