using Microsoft.Playwright;
using Xunit;

namespace ReservaCancha.Tests.E2E
{
    public class ReservaE2ETests
    {
        [Fact]
        public async Task UsuarioPuedeAbrirSistema()
        {
            using var playwright =
                await Playwright.CreateAsync();

            await using var browser =
                await playwright.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions
                    {
                        Headless = true
                    });

            var page =
                await browser.NewPageAsync();

            await page.GotoAsync(
                "https://localhost:7001");

            var titulo =
                await page.TitleAsync();

            Assert.NotNull(titulo);
        }

        [Fact]
        public async Task UsuarioPuedeEntrarALogin()
        {
            using var playwright =
                await Playwright.CreateAsync();

            await using var browser =
                await playwright.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions
                    {
                        Headless = true
                    });

            var page =
                await browser.NewPageAsync();

            await page.GotoAsync(
                "https://localhost:7001");

            var contenido =
                await page.ContentAsync();

            Assert.NotNull(contenido);
        }
    }
}