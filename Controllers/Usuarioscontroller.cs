using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaCancha.Data;
using ReservaCancha.Models;
using System.Security.Cryptography;
using System.Text;

namespace ReservaCancha.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // POST api/usuarios/registrar
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistroRequest request)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(request.Nombre)   ||
                string.IsNullOrWhiteSpace(request.Correo)   ||
                string.IsNullOrWhiteSpace(request.Telefono) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { mensaje = "Todos los campos son requeridos." });
            }

            // RF-1 Backend: Validar que el correo no exista
            bool correoExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo.ToLower() == request.Correo.ToLower());

            if (correoExiste)
            {
                return Conflict(new { mensaje = "El correo ya está registrado. Usa otro o inicia sesión." });
            }

            // Hashear la contraseña (SHA-256 básico; en producción usa BCrypt o Argon2)
            string passwordHash = HashPassword(request.Password);

            // RF-1 Backend: Crear usuario en la base de datos
            var nuevoUsuario = new Usuario
            {
                Nombre       = request.Nombre.Trim(),
                Correo       = request.Correo.Trim().ToLower(),
                Telefono     = request.Telefono.Trim(),
                PasswordHash = passwordHash,
                FechaRegistro = DateTime.UtcNow,
                Activo       = true,
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario registrado exitosamente.", id = nuevoUsuario.Id });
        }

        // ── Utilidad: hash de contraseña ────────────────────────────────
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }

    // ── DTOs ────────────────────────────────────────────────────────────
    public class RegistroRequest
    {
        public string Nombre   { get; set; } = string.Empty;
        public string Correo   { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}