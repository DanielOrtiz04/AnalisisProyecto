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

        public CanchasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCanchas()
        {
            var canchas = await _context.Canchas.ToListAsync();
            return Ok(canchas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCancha(int id)
        {
            var cancha = await _context.Canchas.FindAsync(id);
            if (cancha == null) return NotFound();
            return Ok(cancha);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCancha([FromBody] Cancha cancha)
        {
            _context.Canchas.Add(cancha);
            await _context.SaveChangesAsync();
            return Ok(cancha);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarCancha(int id, [FromBody] Cancha cancha)
        {
            if (id != cancha.Id) return BadRequest();
            _context.Entry(cancha).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(cancha);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCancha(int id)
        {
            var cancha = await _context.Canchas.FindAsync(id);
            if (cancha == null) return NotFound();
            _context.Canchas.Remove(cancha);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("filtrar")]
        public async Task<IActionResult> FiltrarCanchas(
            [FromQuery] string? tipo,
            [FromQuery] bool? disponible)
        {

            var query = _context.Canchas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tipo))
                query = query.Where(c => c.Tipo.ToLower().Contains(tipo.ToLower()));

            if (disponible.HasValue)
                query = query.Where(c => c.Disponible == disponible.Value);

            var resultado = await query.ToListAsync();
            return Ok(resultado);
        }
        [HttpGet("{canchaId}/disponibilidad")]
        public async Task<IActionResult> GetDisponibilidad(int canchaId, [FromQuery] DateTime fecha)
        {
            
            var reservas = await _context.Reservas
                .Where(r => r.CanchaId == canchaId && r.Fecha.Date == fecha.Date)
                .Select(r => r.HoraInicio)
                .ToListAsync();

            var todosHorarios = Enumerable.Range(8, 14)
                .Select(h => TimeSpan.FromHours(h))
                .ToList();

            var disponibles = todosHorarios
                .Where(h => !reservas.Contains(h))
                .Select(h => h.ToString(@"hh\:mm"))
                .ToList();

            return Ok(disponibles);
        }
    }
}