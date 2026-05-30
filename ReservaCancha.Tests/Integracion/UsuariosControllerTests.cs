using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaCancha.Controllers;
using ReservaCancha.Data;
using ReservaCancha.Models;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace ReservaCancha.Tests.Integracion
{
    public class UsuariosControllerTests
    {
        private AppDbContext ObtenerDbContext()
        {
            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private string Hash(string password)
        {
            using var sha = SHA256.Create();

            return Convert.ToHexString(
                sha.ComputeHash(
                    Encoding.UTF8.GetBytes(password)))
                .ToLower();
        }

        [Fact]
        public async Task Registrar_UsuarioNuevo_DebeRetornarOK()
        {
            var context = ObtenerDbContext();

            var controller =
                new UsuariosController(context);

            var request = new RegistroRequest
            {
                Nombre = "Julio",
                Correo = "julio@gmail.com",
                Telefono = "12345678",
                Password = "Password1"
            };

            var resultado =
                await controller.Registrar(request);

            Assert.IsType<OkObjectResult>(resultado);
        }

        [Fact]
        public async Task Registrar_CamposVacios_DebeRetornarBadRequest()
        {
            var context = ObtenerDbContext();

            var controller =
                new UsuariosController(context);

            var request = new RegistroRequest();

            var resultado =
                await controller.Registrar(request);

            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public async Task Registrar_ContrasenaDebil_DebeRetornarBadRequest()
        {
            var context = ObtenerDbContext();

            var controller =
                new UsuariosController(context);

            var request = new RegistroRequest
            {
                Nombre = "Julio",
                Correo = "julio@gmail.com",
                Telefono = "12345678",
                Password = "abc"
            };

            var resultado =
                await controller.Registrar(request);

            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public async Task Registrar_CorreoDuplicado_DebeRetornarConflict()
        {
            var context = ObtenerDbContext();

            context.Usuarios.Add(new Usuario
            {
                Nombre = "Julio",
                Correo = "julio@gmail.com",
                Telefono = "12345678",
                PasswordHash = Hash("Password1"),
                FechaRegistro = DateTime.UtcNow,
                Activo = true
            });

            await context.SaveChangesAsync();

            var controller =
                new UsuariosController(context);

            var request = new RegistroRequest
            {
                Nombre = "Otro",
                Correo = "julio@gmail.com",
                Telefono = "99999999",
                Password = "Password1"
            };

            var resultado =
                await controller.Registrar(request);

            Assert.IsType<ConflictObjectResult>(resultado);
        }

        [Fact]
        public async Task Login_Correcto_DebeRetornarOK()
        {
            var context = ObtenerDbContext();

            context.Usuarios.Add(new Usuario
            {
                Nombre = "Julio",
                Correo = "julio@gmail.com",
                Telefono = "12345678",
                PasswordHash = Hash("Password1"),
                FechaRegistro = DateTime.UtcNow,
                Activo = true
            });

            await context.SaveChangesAsync();

            var controller =
                new UsuariosController(context);

            var request = new LoginRequest
            {
                Correo = "julio@gmail.com",
                Password = "Password1"
            };

            var resultado =
                await controller.Login(request);

            Assert.IsType<OkObjectResult>(resultado);
        }

        [Fact]
        public async Task Login_CredencialesIncorrectas_DebeRetornarUnauthorized()
        {
            var context = ObtenerDbContext();

            var controller =
                new UsuariosController(context);

            var request = new LoginRequest
            {
                Correo = "noexiste@gmail.com",
                Password = "123"
            };

            var resultado =
                await controller.Login(request);

            Assert.IsType<UnauthorizedObjectResult>(resultado);
        }

        [Fact]
        public async Task Login_TresIntentos_DebeBloquearCuenta()
        {
            var context = ObtenerDbContext();

            var usuario = new Usuario
            {
                Nombre = "Julio",
                Correo = "julio@gmail.com",
                Telefono = "12345678",
                PasswordHash = Hash("Password1"),
                FechaRegistro = DateTime.UtcNow,
                Activo = true
            };

            context.Usuarios.Add(usuario);

            await context.SaveChangesAsync();

            var controller =
                new UsuariosController(context);

            for (int i = 0; i < 3; i++)
            {
                await controller.Login(new LoginRequest
                {
                    Correo = "julio@gmail.com",
                    Password = "incorrecta"
                });
            }

            var usuarioActualizado =
                await context.Usuarios.FindAsync(usuario.Id);

            Assert.True(usuarioActualizado!.Bloqueado);
        }

        [Fact]
        public async Task Desbloquear_DebeResetearIntentos()
        {
            var context = ObtenerDbContext();

            var usuario = new Usuario
            {
                Nombre = "Julio",
                Correo = "julio@gmail.com",
                Telefono = "12345678",
                PasswordHash = Hash("Password1"),
                FechaRegistro = DateTime.UtcNow,
                Activo = true,
                Bloqueado = true,
                IntentosFallidos = 3
            };

            context.Usuarios.Add(usuario);

            await context.SaveChangesAsync();

            var controller =
                new UsuariosController(context);

            var resultado =
                await controller.Desbloquear(usuario.Id);

            Assert.IsType<OkObjectResult>(resultado);

            var actualizado =
                await context.Usuarios.FindAsync(usuario.Id);

            Assert.False(actualizado!.Bloqueado);
            Assert.Equal(0, actualizado.IntentosFallidos);
        }
    }
}