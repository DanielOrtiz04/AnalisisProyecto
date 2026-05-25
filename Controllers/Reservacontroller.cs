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
        public ReservasController(AppDbContext context) { _context = context; }

        [HttpPost]
        public async Task<IActionResult> CrearReserva([FromBody] ReservaRequest request)
        {
            if (request == null) return BadRequest(new { mensaje = "Datos inválidos." });
            if (request.CanchaId <= 0 || request.UsuarioId <= 0 || request.Fecha == default)
                return BadRequest(new { mensaje = "Todos los campos son requeridos." });
            if (request.HoraFin <= request.HoraInicio)
                return BadRequest(new { mensaje = "La hora de fin debe ser mayor a la hora de inicio." });

            // RNF-3 Backend: SQLite no traduce .Date ni TimeSpan en LINQ
            // Cargamos solo las reservas de esa cancha en esa fecha (índice IX_Reservas_CanchaId_Fecha)
            // y filtramos el solapamiento de horas en memoria
            var fechaStr = request.Fecha.Date.ToString("yyyy-MM-dd");
            var candidatas = await _context.Reservas
                .AsNoTracking()
                .Where(r => r.CanchaId == request.CanchaId && r.Estado == "Confirmada")
                .Select(r => new { r.Fecha, r.HoraInicio, r.HoraFin })
                .ToListAsync();

            bool horarioOcupado = candidatas.Any(r =>
                r.Fecha.Date == request.Fecha.Date &&
                r.HoraInicio < request.HoraFin &&
                r.HoraFin > request.HoraInicio);

            if (horarioOcupado)
                return Conflict(new { mensaje = "El horario seleccionado ya está reservado." });

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
            return Ok(new { mensaje = "Reserva confirmada exitosamente.", reserva.Id });
        }

        [HttpGet("reporte")]
        public async Task<IActionResult> GetReporteReservas()
        {
            var total         = await _context.Reservas.AsNoTracking().CountAsync();
            var confirmadas   = await _context.Reservas.AsNoTracking().CountAsync(r => r.Estado == "Confirmada");
            var canceladas    = await _context.Reservas.AsNoTracking().CountAsync(r => r.Estado == "Cancelada");
            var porCancha     = await _context.Reservas.AsNoTracking()
                .GroupBy(r => r.CanchaId)
                .Select(g => new { CanchaId = g.Key, Total = g.Count() })
                .ToListAsync();
            var detalle = await _context.Reservas.AsNoTracking()
                .Select(r => new { r.Id, r.CanchaId, r.UsuarioId,
                    Fecha      = r.Fecha.ToString(),
                    HoraInicio = r.HoraInicio.ToString(),
                    HoraFin    = r.HoraFin.ToString(),
                    r.Estado })
                .OrderByDescending(r => r.Id).ToListAsync();
            return Ok(new { totalReservas = total, totalConfirmadas = confirmadas, totalCanceladas = canceladas, porCancha, detalle });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReserva(int id)
        {
            var r = await _context.Reservas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound(new { mensaje = "Reserva no encontrada." });
            return Ok(new { r.Id, r.CanchaId, r.UsuarioId, r.Fecha,
                HoraInicio = r.HoraInicio.ToString(@"hh\:mm"),
                HoraFin    = r.HoraFin.ToString(@"hh\:mm"),
                r.Estado });
        }

        // RNF-3 Backend: índice IX_Reservas_UsuarioId
        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetReservasUsuario(int usuarioId)
        {
            var data = await _context.Reservas.AsNoTracking()
                .Where(r => r.UsuarioId == usuarioId)
                .Select(r => new { r.Id, r.CanchaId, r.Fecha,
                    HoraInicio = r.HoraInicio.ToString(),
                    HoraFin    = r.HoraFin.ToString(),
                    r.Estado })
                .OrderByDescending(r => r.Id).ToListAsync();
            return Ok(new { mensaje = "OK", data });
        }

        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> CancelarReserva(int id)
        {
            var r = await _context.Reservas.FindAsync(id);
            if (r == null) return NotFound(new { mensaje = "Reserva no encontrada." });
            if (r.Estado == "Cancelada") return BadRequest(new { mensaje = "Ya está cancelada." });
            r.Estado = "Cancelada";
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Reserva cancelada exitosamente." });
        }

        [HttpPatch("{id}/modificar")]
        public async Task<IActionResult> ModificarReserva(int id, [FromBody] ModificarRequest request)
        {
            if (request == null) return BadRequest(new { mensaje = "Datos inválidos." });
            if (request.HoraFin <= request.HoraInicio)
                return BadRequest(new { mensaje = "La hora de fin debe ser mayor a la de inicio." });
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null) return NotFound(new { mensaje = "Reserva no encontrada." });
            if (reserva.Estado == "Cancelada") return BadRequest(new { mensaje = "No se puede modificar una reserva cancelada." });

            // Mismo patrón: cargar candidatas en memoria para comparar TimeSpan
            var candidatas = await _context.Reservas.AsNoTracking()
                .Where(r => r.Id != id && r.CanchaId == reserva.CanchaId && r.Estado == "Confirmada")
                .Select(r => new { r.Fecha, r.HoraInicio, r.HoraFin })
                .ToListAsync();

            bool ocupado = candidatas.Any(r =>
                r.Fecha.Date == request.Fecha.Date &&
                r.HoraInicio < request.HoraFin &&
                r.HoraFin > request.HoraInicio);

            if (ocupado) return Conflict(new { mensaje = "El horario seleccionado ya está ocupado." });

            reserva.Fecha      = request.Fecha.Date;
            reserva.HoraInicio = request.HoraInicio;
            reserva.HoraFin    = request.HoraFin;
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Reserva modificada exitosamente." });
        }

        [HttpGet("disponibilidad")]
        public async Task<IActionResult> GetDisponibilidad([FromQuery] int canchaId, [FromQuery] DateTime fecha)
        {
            var reservas = await _context.Reservas.AsNoTracking()
                .Where(r => r.CanchaId == canchaId && r.Estado == "Confirmada")
                .Select(r => new { r.Fecha, r.HoraInicio, r.HoraFin })
                .ToListAsync();
            var del_dia = reservas.Where(r => r.Fecha.Date == fecha.Date)
                .Select(r => new { r.HoraInicio, r.HoraFin });
            return Ok(del_dia);
        }

        [HttpGet]
        public async Task<IActionResult> GetTodasLasReservas()
        {
            var data = await _context.Reservas.AsNoTracking()
                .Select(r => new { r.Id, r.CanchaId, r.UsuarioId,
                    Fecha      = r.Fecha.ToString(),
                    HoraInicio = r.HoraInicio.ToString(),
                    HoraFin    = r.HoraFin.ToString(),
                    r.Estado })
                .OrderByDescending(r => r.Id).ToListAsync();
            return Ok(new { mensaje = "OK", data });
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

    public class ModificarRequest
    {
        public DateTime Fecha      { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin    { get; set; }
    }
}