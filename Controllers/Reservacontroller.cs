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
            if (request == null)
                return BadRequest(new { mensaje = "Datos inválidos." });

            if (request.CanchaId <= 0 ||
                request.UsuarioId <= 0 ||
                request.Fecha == default)
            {
                return BadRequest(new
                {
                    mensaje = "Todos los campos son requeridos."
                });
            }

            if (request.HoraFin <= request.HoraInicio)
            {
                return BadRequest(new
                {
                    mensaje = "La hora de fin debe ser mayor a la hora de inicio."
                });
            }

            bool horarioOcupado = await _context.Reservas.AnyAsync(r =>
                r.CanchaId == request.CanchaId &&
                r.Fecha.Date == request.Fecha.Date &&
                r.Estado == "Confirmada" &&
                r.HoraInicio < request.HoraFin &&
                r.HoraFin > request.HoraInicio
            );

            if (horarioOcupado)
            {
                return Conflict(new
                {
                    mensaje = "El horario seleccionado ya está reservado."
                });
            }

            var reserva = new Reserva
            {
                CanchaId = request.CanchaId,
                UsuarioId = request.UsuarioId,
                Fecha = request.Fecha.Date,
                HoraInicio = request.HoraInicio,
                HoraFin = request.HoraFin,
                Estado = "Confirmada"
            };

            _context.Reservas.Add(reserva);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Reserva confirmada exitosamente.",
                reserva.Id
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound(new
                {
                    mensaje = "Reserva no encontrada."
                });
            }

            return Ok(new
            {
                reserva.Id,
                reserva.CanchaId,
                reserva.UsuarioId,
                reserva.Fecha,
                reserva.HoraInicio,
                reserva.HoraFin,
                reserva.Estado
            });
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetReservasUsuario(int usuarioId)
        {
            var resultado = await _context.Reservas
                .Where(r => r.UsuarioId == usuarioId && r.Estado != null)
                .Select(r => new
                {
                    r.Id,
                    r.CanchaId,
                    Fecha      = r.Fecha.ToString(),
                    HoraInicio = r.HoraInicio.ToString(),
                    HoraFin    = r.HoraFin.ToString(),
                    r.Estado
                })
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            if (!resultado.Any())
                return Ok(new { mensaje = "No se encontraron reservas para este usuario.", data = resultado });

            return Ok(new { mensaje = "Reservas obtenidas correctamente.", data = resultado });
        }

        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> CancelarReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound(new
                {
                    mensaje = "Reserva no encontrada."
                });
            }

            if (reserva.Estado == "Cancelada")
            {
                return BadRequest(new
                {
                    mensaje = "La reserva ya está cancelada."
                });
            }

            reserva.Estado = "Cancelada";

            _context.Reservas.Update(reserva);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Reserva cancelada exitosamente."
            });
        }

        [HttpPatch("{id}/modificar")]
        public async Task<IActionResult> ModificarReserva(int id, [FromBody] ModificarRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    mensaje = "Datos inválidos."
                });
            }

            if (request.HoraFin <= request.HoraInicio)
            {
                return BadRequest(new
                {
                    mensaje = "La hora de fin debe ser mayor a la hora de inicio."
                });
            }

            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound(new
                {
                    mensaje = "Reserva no encontrada."
                });
            }

            if (reserva.Estado == "Cancelada")
            {
                return BadRequest(new
                {
                    mensaje = "No se puede modificar una reserva cancelada."
                });
            }

            bool horarioOcupado = await _context.Reservas.AnyAsync(r =>
                r.Id != id &&
                r.CanchaId == reserva.CanchaId &&
                r.Fecha.Date == request.Fecha.Date &&
                r.Estado == "Confirmada" &&
                r.HoraInicio < request.HoraFin &&
                r.HoraFin > request.HoraInicio
            );

            if (horarioOcupado)
            {
                return Conflict(new
                {
                    mensaje = "El horario seleccionado ya está ocupado."
                });
            }

            reserva.Fecha = request.Fecha.Date;
            reserva.HoraInicio = request.HoraInicio;
            reserva.HoraFin = request.HoraFin;

            _context.Reservas.Update(reserva);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Reserva modificada exitosamente.",
                reserva = new
                {
                    reserva.Id,
                    reserva.Fecha,
                    reserva.HoraInicio,
                    reserva.HoraFin,
                    reserva.Estado
                }
            });
        }

        [HttpGet("disponibilidad")]
        public async Task<IActionResult> GetDisponibilidad(
            [FromQuery] int canchaId,
            [FromQuery] DateTime fecha)
        {
            var reservas = await _context.Reservas
                .Where(r =>
                    r.CanchaId == canchaId &&
                    r.Fecha.Date == fecha.Date &&
                    r.Estado == "Confirmada")
                .Select(r => new
                {
                    r.HoraInicio,
                    r.HoraFin
                })
                .ToListAsync();

            return Ok(reservas);
        }
    
    // Aqui agregue el RF-10: agrego comentrario 
        [HttpGet]
        public async Task<IActionResult> GetTodasLasReservas()
        {
            var reservas = await _context.Reservas
                .Select(r => new
                {
                    r.Id,
                    r.CanchaId,
                    r.UsuarioId,
                    Fecha      = r.Fecha.ToString("yyyy-MM-dd"),
                    HoraInicio = r.HoraInicio.ToString(),
                    HoraFin    = r.HoraFin.ToString(),
                    r.Estado
                })
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return Ok(new { mensaje = "Reservas obtenidas correctamente.", data = reservas });
        }

    }

    public class ReservaRequest
    {
        public int CanchaId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }

    public class ModificarRequest
    {
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }
}

