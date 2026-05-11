using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaCancha.Data;
using ReservaCancha.Models;

namespace ReservaCancha.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CrearReserva([FromBody] ReservaRequest request)
        {
            if (request.CanchaId <= 0 || request.Fecha == default || request.HoraInicio == default || request.HoraFin == default)
                return BadRequest(new { mensaje = "Todos los campos son requeridos." });

            if (request.HoraFin <= request.HoraInicio)
                return BadRequest(new { mensaje = "La hora de fin debe ser mayor a la hora de inicio." });

            bool horarioOcupado = await _context.Reservas.AnyAsync(r =>
                r.CanchaId == request.CanchaId &&
                r.Fecha.Date == request.Fecha.Date &&
                r.Estado == "Confirmada" &&
                r.HoraInicio < request.HoraFin &&
                r.HoraFin > request.HoraInicio
            );

            if (horarioOcupado)
                return Conflict(new { mensaje = "El horario seleccionado ya esta reservado. Elige otro." });

            var reserva = new Reserva
            {
                CanchaId   = request.CanchaId,
                UsuarioId  = request.UsuarioId,
                Fecha      = request.Fecha.Date,
                HoraInicio = request.HoraInicio,
                HoraFin    = request.HoraFin,
                Estado     = "Confirmada"
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Reserva confirmada exitosamente.", id = reserva.Id });
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetReservasUsuario(int usuarioId)
        {
            var reservas = await _context.Reservas
                .Where(r => r.UsuarioId == usuarioId)
                .OrderByDescending(r => r.Fecha)
                .Select(r => new
                {
                    r.Id,
                    r.CanchaId,
                    r.Fecha,
                    r.HoraInicio,
                    r.HoraFin,
                    r.Estado
                })
                .ToListAsync();

            return Ok(reservas);
        }
      
        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> CancelarReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
                return NotFound(new { mensaje = "Reserva no encontrada." });

            if (reserva.Estado == "Cancelada")
                return BadRequest(new { mensaje = "La reserva ya esta cancelada." });

           
            reserva.Estado = "Cancelada";
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Reserva cancelada exitosamente." });
        }
      
        [HttpGet("disponibilidad")]
        public async Task<IActionResult> GetDisponibilidad([FromQuery] int canchaId, [FromQuery] DateTime fecha)
        {
            var reservas = await _context.Reservas
                .Where(r => r.CanchaId == canchaId && r.Fecha.Date == fecha.Date && r.Estado == "Confirmada")
                .Select(r => new { r.HoraInicio, r.HoraFin })
                .ToListAsync();

            return Ok(reservas);
        }
    }

    public class ReservaRequest
    {
        public int      CanchaId   { get; set; }
        public int      UsuarioId  { get; set; }
        public DateTime Fecha      { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin    { get; set; }
    }
}