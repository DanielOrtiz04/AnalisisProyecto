using ReservaCancha.Models;

namespace ReservaCancha.Services
{
    public enum TipoNotificacion
    {
        Exito,
        Error,
        Advertencia
    }

    public class NotificacionReserva
    {
        public string Mensaje { get; set; } = string.Empty;
        public TipoNotificacion Tipo { get; set; }
        public DateTime FechaHora { get; set; } = DateTime.Now;
    }

    public class NotificacionReservaService
    {
        // RF-12: Detectar el tipo de cambio en la reserva y generar el mensaje correspondiente
        public NotificacionReserva GenerarNotificacionCreacion(int reservaId, int canchaId, DateTime fecha)
        {
            return new NotificacionReserva
            {
                Mensaje = $"Reserva #{reservaId} confirmada exitosamente para la Cancha #{canchaId} el {fecha:dd/MM/yyyy}.",
                Tipo    = TipoNotificacion.Exito
            };
        }

        public NotificacionReserva GenerarNotificacionModificacion(int reservaId, DateTime nuevaFecha, TimeSpan horaInicio, TimeSpan horaFin)
        {
            return new NotificacionReserva
            {
                Mensaje = $"Reserva #{reservaId} modificada. Nueva fecha: {nuevaFecha:dd/MM/yyyy} de {horaInicio:hh\\:mm} a {horaFin:hh\\:mm}.",
                Tipo    = TipoNotificacion.Exito
            };
        }

        public NotificacionReserva GenerarNotificacionCancelacion(int reservaId)
        {
            return new NotificacionReserva
            {
                Mensaje = $"Reserva #{reservaId} cancelada correctamente.",
                Tipo    = TipoNotificacion.Exito
            };
        }

        public NotificacionReserva GenerarNotificacionError(string detalle)
        {
            return new NotificacionReserva
            {
                Mensaje = detalle,
                Tipo    = TipoNotificacion.Error
            };
        }

        // RF-12: Enviar información de estado al frontend — construye el objeto de respuesta
        public object ConstruirRespuestaEstado(NotificacionReserva notificacion, object? datosReserva = null)
        {
            return new
            {
                tipo      = notificacion.Tipo.ToString().ToLower(),
                mensaje   = notificacion.Mensaje,
                fechaHora = notificacion.FechaHora,
                datos     = datosReserva
            };
        }
    }
}