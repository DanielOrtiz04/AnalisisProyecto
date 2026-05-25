namespace ReservaCancha.Services
{
    // Servicio singleton que guarda la sesión en memoria durante la vida del servidor
    public class SesionService
    {
        public int    UsuarioId  { get; private set; }
        public string Nombre     { get; private set; } = string.Empty;
        public string Correo     { get; private set; } = string.Empty;
        public bool   EsAdmin    { get; private set; }
        public bool   Autenticado => UsuarioId > 0;

        private const string AdminCorreo     = "derekmarmol236@gmail.com";

        public event Action? OnCambio;

        public void IniciarSesion(int id, string nombre, string correo)
        {
            UsuarioId = id;
            Nombre    = nombre;
            Correo    = correo;
            EsAdmin   = correo.Trim().ToLower() == AdminCorreo.ToLower();
            OnCambio?.Invoke();
        }

        public void CerrarSesion()
        {
            UsuarioId = 0;
            Nombre    = string.Empty;
            Correo    = string.Empty;
            EsAdmin   = false;
            OnCambio?.Invoke();
        }
    }
}
