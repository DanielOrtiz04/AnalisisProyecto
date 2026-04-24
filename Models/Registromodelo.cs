using System.ComponentModel.DataAnnotations;

namespace ReservaCancha.Models
{
    public class RegistroModelo
    {
        [Required(ErrorMessage = "El nombre es requerido.")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido.")]
        [Phone(ErrorMessage = "Ingresa un número de teléfono válido.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma tu contraseña.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}