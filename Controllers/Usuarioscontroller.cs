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
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Correo) ||
                string.IsNullOrWhiteSpace(request.Telefono) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { mensaje = "Todos los campos son requeridos." });
            }

            bool correoExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo.ToLower() == request.Correo.ToLower());

            if (correoExiste)
            {
                return Conflict(new { mensaje = "El correo ya está registrado. Usa otro o inicia sesión." });
            }

            string passwordHash = HashPassword(request.Password);

            var nuevoUsuario = new Usuario
            {
                Nombre = request.Nombre.Trim(),
                Correo = request.Correo.Trim().ToLower(),
                Telefono = request.Telefono.Trim(),
                PasswordHash = passwordHash,
                FechaRegistro = DateTime.UtcNow,
                Activo = true,
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario registrado exitosamente.", id = nuevoUsuario.Id });
        }

        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { mensaje = "Correo y contraseña son requeridos." });
            }

            string hashIngresado = HashPassword(request.Password);
            string correoNormalizado = request.Correo.Trim().ToLower();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correoNormalizado && u.PasswordHash == hashIngresado);

            if (usuario == null)
            {
                return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
            }

            return Ok(new { mensaje = "Acceso exitoso", usuarioId = usuario.Id, nombre = usuario.Nombre });
        }

        
        [HttpGet("{id}/perfil")]
        public async Task<IActionResult> GetPerfil(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            return Ok(new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Correo,
                usuario.Telefono,
                usuario.FechaRegistro,
                usuario.Activo
            });
        }

        [HttpPut("{id}/perfil")]
        public async Task<IActionResult> ActualizarPerfil(int id, [FromBody] ActualizarPerfilRequest request)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Telefono))
            {
                return BadRequest(new { mensaje = "Nombre y teléfono son requeridos." });
            }

            usuario.Nombre = request.Nombre.Trim();
            usuario.Telefono = request.Telefono.Trim();

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Perfil actualizado correctamente." });
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }

    public class RegistroRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class ActualizarPerfilRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }
}