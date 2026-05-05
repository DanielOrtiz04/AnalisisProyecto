namespace ReservaCancha.Models
{
    public class Reserva
    {
        public int      Id         { get; set; }
        public int      CanchaId   { get; set; }
        public Cancha   Cancha     { get; set; } = null!;
        public int      UsuarioId  { get; set; }
        public DateTime Fecha      { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin    { get; set; }
        public string   Estado     { get; set; } = "Confirmada";
    }
}