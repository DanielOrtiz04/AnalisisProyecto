using Microsoft.EntityFrameworkCore;
using ReservaCancha.Data;
using ReservaCancha.Models;
using ReservaCancha.Services;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<NotificacionReservaService>();
builder.Services.AddSingleton<SesionService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("PoliticaSegura", policy =>
        policy.WithOrigins("https://localhost:7001", "http://localhost:5085")
              .AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddAntiforgery(options => { options.HeaderName = "X-CSRF-TOKEN"; });
builder.Services.AddHealthChecks();

var app = builder.Build();

// Crear DB y sembrar admin hardcodeado
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    const string adminCorreo = "derekmarmol236@gmail.com";
    if (!db.Usuarios.Any(u => u.Correo == adminCorreo))
    {
        using var sha = SHA256.Create();
        var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes("Marmol4812"))).ToLower();
        db.Usuarios.Add(new Usuario
        {
            Nombre        = "Admin",
            Correo        = adminCorreo,
            Telefono      = "00000000",
            PasswordHash  = hash,
            FechaRegistro = DateTime.UtcNow,
            Activo        = true,
        });
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("PoliticaSegura");
app.MapHealthChecks("/health");
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
