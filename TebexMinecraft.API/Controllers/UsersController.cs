using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using TebexMinecraft.API.Data;
using TebexMinecraft.Modelos;

namespace TebexMinecraft.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly TebexMinecraftAPIContext _context;
    private static readonly HttpClient _httpClient = new HttpClient();

    static UsersController()
    {
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TebexMinecraft-API/1.0");
    }

    public UsersController(TebexMinecraftAPIContext context)
    {
        _context = context;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.AsNoTracking().ToListAsync();
    }

    // GET: api/Users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        return user;
    }

    // GET: api/Users/by-uuid/{uuid}
    [HttpGet("by-uuid/{uuid}")]
    public async Task<ActionResult<User>> GetUserByUuid(string uuid)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Uuid == uuid);
        if (user == null) return NotFound();
        return user;
    }
    
    // GET: api/Users/5/ranks
    [HttpGet("{id}/ranks")]
    public async Task<ActionResult<IEnumerable<string>>> GetUserRanks(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound($"Usuario con ID {id} no encontrado.");
        }

        var ranks = await _context.UserRanks
            .Where(ur => ur.Uuid == user.Uuid)
            .Select(ur => ur.ServerRank)
            .ToListAsync();

        return Ok(ranks);
    }
    
    // GET: api/Users/5/prefixes
    [HttpGet("{id}/prefixes")]
    public async Task<ActionResult<IEnumerable<string>>> GetUserPrefixes(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound($"Usuario con ID {id} no encontrado.");
        }

        var prefixes = await _context.UserPrefixes
            .Where(up => up.Uuid == user.Uuid)
            .Select(up => up.ServerTag)
            .ToListAsync();

        return Ok(prefixes);
    }

    // POST: api/Users/lookup/{username}
    [HttpPost("lookup/{username}")]
    public async Task<ActionResult<User>> LookupMojang(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest("El nombre de usuario no puede estar vacío.");
        }

        try
        {
            var mojangUrl = $"https://api.mojang.com/users/profiles/minecraft/{username.Trim()}";
            var response = await _httpClient.GetAsync(mojangUrl);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent || 
                response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound("El jugador no fue encontrado en los servidores de Mojang.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error en la comunicación con Mojang.");
            }

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            string rawId = json["id"]?.ToString() ?? string.Empty;
            string exactName = json["name"]?.ToString() ?? username.Trim();

            if (string.IsNullOrEmpty(rawId) || rawId.Length != 32)
            {
                return BadRequest("Formato de UUID devuelto por Mojang inválido.");
            }
            
            string formattedUuid = $"{rawId[..8]}-{rawId[8..12]}-{rawId[12..16]}-{rawId[16..20]}-{rawId[20..]}";
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uuid == formattedUuid);
            if (user == null)
            {
                user = new User
                {
                    Uuid = formattedUuid,
                    Username = exactName
                };
                _context.Users.Add(user);
            }
            else
            {
                user.Username = exactName;
            }

            await _context.SaveChangesAsync();
            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al verificar con Mojang: {ex.Message}");
        }
    }

    // PUT: api/Users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(long id, User user)
    {
        if (id != user.Id) return BadRequest();

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Users.AnyAsync(e => e.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    // POST: api/Users
    [HttpPost]
    public async Task<ActionResult<User>> PostUser(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // DELETE: api/Users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}