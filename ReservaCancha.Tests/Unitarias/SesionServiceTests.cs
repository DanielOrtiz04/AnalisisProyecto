using ReservaCancha.Services;
using Xunit;

namespace ReservaCancha.Tests.Unitarias
{
    public class SesionServiceTests
    {
        [Fact]
        public void IniciarSesion_DebeAutenticarUsuario()
        {
            var service = new SesionService();

            service.IniciarSesion(
                1,
                "Julio",
                "julio@gmail.com");

            Assert.True(service.Autenticado);
            Assert.Equal("Julio", service.Nombre);
        }

        [Fact]
        public void CerrarSesion_DebeLimpiarSesion()
        {
            var service = new SesionService();

            service.IniciarSesion(
                1,
                "Julio",
                "julio@gmail.com");

            service.CerrarSesion();

            Assert.False(service.Autenticado);
            Assert.Equal("", service.Nombre);
        }

        [Fact]
        public void IniciarSesion_AdminDebeSerVerdadero()
        {
            var service = new SesionService();

            service.IniciarSesion(
                1,
                "Admin",
                "derekmarmol236@gmail.com");

            Assert.True(service.EsAdmin);
        }

        [Fact]
        public void IniciarSesion_UsuarioNormal_NoEsAdmin()
        {
            var service = new SesionService();

            service.IniciarSesion(
                1,
                "Julio",
                "julio@gmail.com");

            Assert.False(service.EsAdmin);
        }
    }
}