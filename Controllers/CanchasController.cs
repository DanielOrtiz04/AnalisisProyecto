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