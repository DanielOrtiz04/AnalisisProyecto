namespace ReservaCancha.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public int CanchaId { get; set; }

        public Cancha? Cancha { get; set; }

        public DateTime Fecha { get; set; }

        public string Horario { get; set; } = string.Empty;

        public string Estado { get; set; } = "Pendiente";
    }
}