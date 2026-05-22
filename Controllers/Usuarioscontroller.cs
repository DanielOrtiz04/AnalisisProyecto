using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaCancha.Data;
using ReservaCancha.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ReservaCancha.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private const int MaxIntentosFallidos = 3;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

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

            if (!ContrasenaSegura(request.Password))
            {
                return BadRequest(new { mensaje = "La contraseña debe tener mínimo 8 caracteres, una mayúscula y un número." });
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
                IntentosFallidos = 0,
                Bloqueado = false,
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

            string correoNormalizado = request.Correo.Trim().ToLower();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correoNormalizado);

            if (usuario == null)
            {
                return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
            }

            if (usuario.Bloqueado)
            {
                return Unauthorized(new { mensaje = "Cuenta bloqueada por demasiados intentos fallidos. Contacta al administrador." });
            }

            string hashIngresado = HashPassword(request.Password);

            if (usuario.PasswordHash != hashIngresado)
            {
                usuario.IntentosFallidos++;

                if (usuario.IntentosFallidos >= MaxIntentosFallidos)
                {
                    usuario.Bloqueado = true;
                    usuario.FechaBloqueo = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Unauthorized(new { mensaje = "Cuenta bloqueada por 3 intentos fallidos. Contacta al administrador." });
                }

                await _context.SaveChangesAsync();
                int intentosRestantes = MaxIntentosFallidos - usuario.IntentosFallidos;
                return Unauthorized(new { mensaje = $"Contraseña incorrecta. Te quedan {intentosRestantes} intento(s)." });
            }

            usuario.IntentosFallidos = 0;
            usuario.Bloqueado = false;
            await _context.SaveChangesAsync();

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

        // PUT api/usuarios/{id}/perfil
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

        [HttpPost("{id}/desbloquear")]
        public async Task<IActionResult> Desbloquear(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            usuario.Bloqueado = false;
            usuario.IntentosFallidos = 0;
            usuario.FechaBloqueo = null;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cuenta desbloqueada correctamente." });
        }

        private static bool ContrasenaSegura(string password)
        {
            if (password.Length < 8) return false;
            if (!Regex.IsMatch(password, @"[A-Z]")) return false;
            if (!Regex.IsMatch(password, @"[0-9]")) return false;
            return true;
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