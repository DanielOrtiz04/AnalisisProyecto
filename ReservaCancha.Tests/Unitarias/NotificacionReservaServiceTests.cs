using ReservaCancha.Services;
using Xunit;

namespace ReservaCancha.Tests.Unitarias
{
    public class NotificacionReservaServiceTests
    {
        [Fact]
        public void GenerarNotificacionCreacion_RetornaMensajeCorrecto()
        {
            var service = new NotificacionReservaService();

            var resultado = service.GenerarNotificacionCreacion(
                1,
                2,
                new DateTime(2026, 5, 29));

            Assert.Equal(
                "Reserva #1 confirmada exitosamente para la Cancha #2 el 29/05/2026.",
                resultado.Mensaje);

            Assert.Equal(TipoNotificacion.Exito, resultado.Tipo);
        }

        [Fact]
        public void GenerarNotificacionModificacion_RetornaMensajeCorrecto()
        {
            var service = new NotificacionReservaService();

            var resultado = service.GenerarNotificacionModificacion(
                1,
                new DateTime(2026, 5, 29),
                new TimeSpan(10, 0, 0),
                new TimeSpan(11, 0, 0));

            Assert.Contains("modificada", resultado.Mensaje);
            Assert.Equal(TipoNotificacion.Exito, resultado.Tipo);
        }

        [Fact]
        public void GenerarNotificacionCancelacion_RetornaMensajeCorrecto()
        {
            var service = new NotificacionReservaService();

            var resultado = service.GenerarNotificacionCancelacion(1);

            Assert.Equal(
                "Reserva #1 cancelada correctamente.",
                resultado.Mensaje);

            Assert.Equal(TipoNotificacion.Exito, resultado.Tipo);
        }

        [Fact]
        public void GenerarNotificacionError_RetornaTipoError()
        {
            var service = new NotificacionReservaService();

            var resultado = service.GenerarNotificacionError(
                "Error de prueba");

            Assert.Equal("Error de prueba", resultado.Mensaje);
            Assert.Equal(TipoNotificacion.Error, resultado.Tipo);
        }

        [Fact]
        public void ConstruirRespuestaEstado_RetornaObjeto()
        {
            var service = new NotificacionReservaService();

            var notificacion =
                service.GenerarNotificacionCreacion(
                    1,
                    2,
                    DateTime.Today);

            var resultado =
                service.ConstruirRespuestaEstado(notificacion);

            Assert.NotNull(resultado);
        }
    }
}