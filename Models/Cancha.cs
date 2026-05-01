namespace ReservaCancha.Models
{
    public class Cancha
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool Disponible { get; set; }
    }
}