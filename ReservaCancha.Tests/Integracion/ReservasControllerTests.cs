using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaCancha.Controllers;
using ReservaCancha.Data;
using ReservaCancha.Models;
using Xunit;

namespace ReservaCancha.Tests.Integracion
{
    public class ReservasControllerTests
    {
        private AppDbContext ObtenerDbContext()
        {
            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task CrearReserva_DebeRetornarOK()
        {
            var context = ObtenerDbContext();

            var controller = new ReservasController(context);

            var request = new ReservaRequest
            {
                CanchaId = 1,
                UsuarioId = 1,
                Fecha = DateTime.Today,
                HoraInicio = new TimeSpan(10, 0, 0),
                HoraFin = new TimeSpan(11, 0, 0)
            };

            var resultado = await controller.CrearReserva(request);

            Assert.IsType<OkObjectResult>(resultado);

            Assert.Equal(1, context.Reservas.Count());
        }

        [Fact]
        public async Task CrearReserva_HorarioOcupado_DebeRetornarConflict()
        {
            var context = ObtenerDbContext();

            context.Reservas.Add(new Reserva
            {
                CanchaId = 1,
                UsuarioId = 1,
                Fecha = DateTime.Today,
                HoraInicio = new TimeSpan(10, 0, 0),
                HoraFin = new TimeSpan(11, 0, 0),
                Estado = "Confirmada"
            });

            await context.SaveChangesAsync();

            var controller = new ReservasController(context);

            var request = new ReservaRequest
            {
                CanchaId = 1,
                UsuarioId = 2,
                Fecha = DateTime.Today,
                HoraInicio = new TimeSpan(10, 30, 0),
                HoraFin = new TimeSpan(11, 30, 0)
            };

            var resultado = await controller.CrearReserva(request);

            Assert.IsType<ConflictObjectResult>(resultado);
        }

        [Fact]
        public async Task CancelarReserva_DebeCambiarEstado()
        {
            var context = ObtenerDbContext();

            var reserva = new Reserva
            {
                CanchaId = 1,
                UsuarioId = 1,
                Fecha = DateTime.Today,
                HoraInicio = new TimeSpan(8, 0, 0),
                HoraFin = new TimeSpan(9, 0, 0),
                Estado = "Confirmada"
            };

            context.Reservas.Add(reserva);

            await context.SaveChangesAsync();

            var controller = new ReservasController(context);

            var resultado =
                await controller.CancelarReserva(reserva.Id);

            Assert.IsType<OkObjectResult>(resultado);

            var reservaActualizada =
                await context.Reservas.FindAsync(reserva.Id);

            Assert.Equal(
                "Cancelada",
                reservaActualizada!.Estado);
        }

        [Fact]
        public async Task GetReserva_DebeRetornarReserva()
        {
            var context = ObtenerDbContext();

            var reserva = new Reserva
            {
                CanchaId = 1,
                UsuarioId = 1,
                Fecha = DateTime.Today,
                HoraInicio = new TimeSpan(8, 0, 0),
                HoraFin = new TimeSpan(9, 0, 0),
                Estado = "Confirmada"
            };

            context.Reservas.Add(reserva);

            await context.SaveChangesAsync();

            var controller = new ReservasController(context);

            var resultado =
                await controller.GetReserva(reserva.Id);

            Assert.IsType<OkObjectResult>(resultado);
        }

        [Fact]
        public async Task GetReserva_NoExiste_DebeRetornarNotFound()
        {
            var context = ObtenerDbContext();

            var controller = new ReservasController(context);

            var resultado =
                await controller.GetReserva(999);

            Assert.IsType<NotFoundObjectResult>(resultado);
        }

        [Fact]
        public async Task ModificarReserva_DebeModificarCorrectamente()
        {
            var context = ObtenerDbContext();

            var reserva = new Reserva
            {
                CanchaId = 1,
                UsuarioId = 1,
                Fecha = DateTime.Today,
                HoraInicio = new TimeSpan(8, 0, 0),
                HoraFin = new TimeSpan(9, 0, 0),
                Estado = "Confirmada"
            };

            context.Reservas.Add(reserva);

            await context.SaveChangesAsync();

            var controller = new ReservasController(context);

            var request = new ModificarRequest
            {
                Fecha = DateTime.Today,
                HoraInicio = new TimeSpan(12, 0, 0),
                HoraFin = new TimeSpan(13, 0, 0)
            };

            var resultado =
                await controller.ModificarReserva(
                    reserva.Id,
                    request);

            Assert.IsType<OkObjectResult>(resultado);
        }

        [Fact]
        public async Task Disponibilidad_DebeRetornarHorarios()
        {
            var context = ObtenerDbContext();

            context.Reservas.Add(new Reserva
            {
                CanchaId = 1,
                UsuarioId = 1,
                Fecha = DateTime.Today,
                HoraInicio = new TimeSpan(10, 0, 0),
                HoraFin = new TimeSpan(11, 0, 0),
                Estado = "Confirmada"
            });

            await context.SaveChangesAsync();

            var controller = new ReservasController(context);

            var resultado =
                await controller.GetDisponibilidad(
                    1,
                    DateTime.Today);

            Assert.IsType<OkObjectResult>(resultado);
        }
    }
}