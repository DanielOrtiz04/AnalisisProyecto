namespace ReservaCancha.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public int CanchaId { get; set; }
        public Cancha Cancha { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
    }
}