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

        public UsuariosController(AppDbContext context) { _context = context; }

        // RNF-4 Backend: respuestas con estructura clara y consistente para el frontend
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistroRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre)   ||
                string.IsNullOrWhiteSpace(request.Correo)   ||
                string.IsNullOrWhiteSpace(request.Telefono) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(RespuestaUsabilidad(
                    codigo:          "CAMPOS_REQUERIDOS",
                    mensajeUsuario:  "Por favor completa todos los campos antes de continuar.",
                    mensajeTecnico:  "Campos requeridos faltantes."
                ));
            }

            if (!ContrasenaSegura(request.Password))
            {
                return BadRequest(RespuestaUsabilidad(
                    codigo:          "CONTRASENA_DEBIL",
                    mensajeUsuario:  "La contraseña debe tener al menos 8 caracteres, una letra mayúscula y un número.",
                    mensajeTecnico:  "La contraseña no cumple los requisitos de seguridad."
                ));
            }

            bool correoExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo.ToLower() == request.Correo.ToLower());

            if (correoExiste)
            {
                return Conflict(RespuestaUsabilidad(
                    codigo:          "CORREO_DUPLICADO",
                    mensajeUsuario:  "Ese correo ya tiene una cuenta. ¿Quieres iniciar sesión?",
                    mensajeTecnico:  "El correo ya está registrado en la base de datos."
                ));
            }

            var nuevoUsuario = new Usuario
            {
                Nombre        = request.Nombre.Trim(),
                Correo        = request.Correo.Trim().ToLower(),
                Telefono      = request.Telefono.Trim(),
                PasswordHash  = HashPassword(request.Password),
                FechaRegistro = DateTime.UtcNow,
                Activo        = true,
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(RespuestaUsabilidad(
                codigo:          "REGISTRO_EXITOSO",
                mensajeUsuario:  "¡Cuenta creada! Bienvenido a ReservaCancha.",
                mensajeTecnico:  "Usuario registrado correctamente.",
                datos:           new { id = nuevoUsuario.Id, nombre = nuevoUsuario.Nombre }
            ));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(RespuestaUsabilidad(
                    codigo:         "CAMPOS_REQUERIDOS",
                    mensajeUsuario: "Ingresa tu correo y contraseña para continuar.",
                    mensajeTecnico: "Correo o contraseña vacíos."
                ));
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == request.Correo.Trim().ToLower());

            if (usuario == null)
            {
                return Unauthorized(RespuestaUsabilidad(
                    codigo:         "CREDENCIALES_INCORRECTAS",
                    mensajeUsuario: "El correo o la contraseña son incorrectos.",
                    mensajeTecnico: "Usuario no encontrado."
                ));
            }

            if (usuario.Bloqueado)
            {
                return Unauthorized(RespuestaUsabilidad(
                    codigo:         "CUENTA_BLOQUEADA",
                    mensajeUsuario: "Tu cuenta está bloqueada por demasiados intentos fallidos. Contacta al administrador.",
                    mensajeTecnico: "Cuenta bloqueada."
                ));
            }

            if (usuario.PasswordHash != HashPassword(request.Password))
            {
                usuario.IntentosFallidos++;
                if (usuario.IntentosFallidos >= MaxIntentosFallidos)
                {
                    usuario.Bloqueado    = true;
                    usuario.FechaBloqueo = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Unauthorized(RespuestaUsabilidad(
                        codigo:         "CUENTA_BLOQUEADA",
                        mensajeUsuario: "Cuenta bloqueada tras 3 intentos fallidos. Contacta al administrador.",
                        mensajeTecnico: "Límite de intentos alcanzado."
                    ));
                }
                await _context.SaveChangesAsync();
                int restantes = MaxIntentosFallidos - usuario.IntentosFallidos;
                return Unauthorized(RespuestaUsabilidad(
                    codigo:         "CONTRASENA_INCORRECTA",
                    mensajeUsuario: $"Contraseña incorrecta. Te quedan {restantes} intento(s) antes de bloquear la cuenta.",
                    mensajeTecnico: $"Hash no coincide. Intentos fallidos: {usuario.IntentosFallidos}."
                ));
            }

            usuario.IntentosFallidos = 0;
            usuario.Bloqueado        = false;
            await _context.SaveChangesAsync();

            return Ok(RespuestaUsabilidad(
                codigo:         "LOGIN_EXITOSO",
                mensajeUsuario: $"¡Bienvenido de vuelta, {usuario.Nombre.Split(' ')[0]}!",
                mensajeTecnico: "Autenticación exitosa.",
                datos:          new { usuarioId = usuario.Id, nombre = usuario.Nombre }
            ));
        }

        [HttpGet("{id}/perfil")]
        public async Task<IActionResult> GetPerfil(int id)
        {
            var u = await _context.Usuarios.FindAsync(id);
            if (u == null)
                return NotFound(RespuestaUsabilidad(
                    codigo: "USUARIO_NO_ENCONTRADO",
                    mensajeUsuario: "No encontramos tu perfil. Intenta cerrar sesión e iniciar de nuevo.",
                    mensajeTecnico: $"Usuario con id={id} no existe."
                ));

            // RNF-4 Backend: datos organizados y con nombres descriptivos
            return Ok(new
            {
                Id            = u.Id,
                Nombre        = u.Nombre,
                Correo        = u.Correo,
                Telefono      = u.Telefono,
                FechaRegistro = u.FechaRegistro,
                Activo        = u.Activo,
                EsAdmin       = u.Correo == "derekmarmol236@gmail.com"
            });
        }

        [HttpPut("{id}/perfil")]
        public async Task<IActionResult> ActualizarPerfil(int id, [FromBody] ActualizarPerfilRequest request)
        {
            var u = await _context.Usuarios.FindAsync(id);
            if (u == null)
                return NotFound(RespuestaUsabilidad(
                    codigo: "USUARIO_NO_ENCONTRADO",
                    mensajeUsuario: "No encontramos tu perfil.",
                    mensajeTecnico: $"Usuario {id} no existe."
                ));

            if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Telefono))
                return BadRequest(RespuestaUsabilidad(
                    codigo: "CAMPOS_REQUERIDOS",
                    mensajeUsuario: "El nombre y el teléfono no pueden estar vacíos.",
                    mensajeTecnico: "Nombre o Telefono vacío."
                ));

            u.Nombre   = request.Nombre.Trim();
            u.Telefono = request.Telefono.Trim();
            await _context.SaveChangesAsync();

            return Ok(RespuestaUsabilidad(
                codigo:         "PERFIL_ACTUALIZADO",
                mensajeUsuario: "Tu perfil fue actualizado correctamente.",
                mensajeTecnico: "Campos Nombre y Telefono actualizados."
            ));
        }

        [HttpPost("{id}/desbloquear")]
        public async Task<IActionResult> Desbloquear(int id)
        {
            var u = await _context.Usuarios.FindAsync(id);
            if (u == null) return NotFound(new { mensaje = "Usuario no encontrado." });
            u.Bloqueado = false; u.IntentosFallidos = 0; u.FechaBloqueo = null;
            await _context.SaveChangesAsync();
            return Ok(RespuestaUsabilidad(
                codigo:         "CUENTA_DESBLOQUEADA",
                mensajeUsuario: "La cuenta fue desbloqueada.",
                mensajeTecnico: "IntentosFallidos reseteado."
            ));
        }

        // RNF-4 Backend: estructura de respuesta uniforme y legible
        private static object RespuestaUsabilidad(string codigo, string mensajeUsuario, string mensajeTecnico, object? datos = null) =>
            new
            {
                Codigo         = codigo,
                Mensaje        = mensajeUsuario,   // texto amigable para mostrar en UI
                MensajeTecnico = mensajeTecnico,   // detalle para logs/debug
                Datos          = datos
            };

        private static bool ContrasenaSegura(string p) =>
            p.Length >= 8 && Regex.IsMatch(p, @"[A-Z]") && Regex.IsMatch(p, @"[0-9]");

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(password))).ToLower();
        }
    }

    public class RegistroRequest
    {
        public string Nombre   { get; set; } = string.Empty;
        public string Correo   { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Correo   { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ActualizarPerfilRequest
    {
        public string Nombre   { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }
}