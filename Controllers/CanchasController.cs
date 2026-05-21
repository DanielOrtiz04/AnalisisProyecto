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

        // GET: api/canchas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cancha>>> ObtenerCanchas()
        {
            return await _context.Canchas.ToListAsync();
        }

        // GET: api/canchas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cancha>> ObtenerCancha(int id)
        {
            var cancha = await _context.Canchas.FindAsync(id);

            if (cancha == null)
            {
                return NotFound();
            }

            return cancha;
        }

        // POST: api/canchas
        [HttpPost]
        public async Task<ActionResult<Cancha>> CrearCancha(Cancha cancha)
        {
            _context.Canchas.Add(cancha);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObtenerCancha),
                new { id = cancha.Id },
                cancha
            );
        }

        // PUT: api/canchas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarCancha(int id, Cancha cancha)
        {
            if (id != cancha.Id)
            {
                return BadRequest();
            }

            _context.Entry(cancha).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/canchas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCancha(int id)
        {
            var cancha = await _context.Canchas.FindAsync(id);

            if (cancha == null)
            {
                return NotFound();
            }

            _context.Canchas.Remove(cancha);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}