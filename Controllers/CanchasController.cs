using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaCancha.Data;
using ReservaCancha.Models;

namespace ReservaCancha.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CanchasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CanchasController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cancha>>> ObtenerCanchas()
            => await _context.Canchas.AsNoTracking().ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Cancha>> ObtenerCancha(int id)
        {
            var c = await _context.Canchas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return c == null ? NotFound() : c;
        }

        // RNF-3 + Filtrar Canchas: endpoint con filtros opcionales
        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<Cancha>>> FiltrarCanchas(
            [FromQuery] string? tipo,
            [FromQuery] bool? disponible)
        {
            var query = _context.Canchas.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(tipo))
                query = query.Where(c => c.Tipo.ToLower().Contains(tipo.ToLower()));
            if (disponible.HasValue)
                query = query.Where(c => c.Disponible == disponible.Value);
            return await query.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Cancha>> CrearCancha(Cancha cancha)
        {
            _context.Canchas.Add(cancha);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(ObtenerCancha), new { id = cancha.Id }, cancha);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarCancha(int id, Cancha cancha)
        {
            if (id != cancha.Id) return BadRequest();
            _context.Entry(cancha).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCancha(int id)
        {
            var c = await _context.Canchas.FindAsync(id);
            if (c == null) return NotFound();
            _context.Canchas.Remove(c);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
