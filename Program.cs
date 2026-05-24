using Microsoft.EntityFrameworkCore;
using ReservaCancha.Data;
using ReservaCancha.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<NotificacionReservaService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PoliticaSegura", policy =>
    {
        policy.WithOrigins("https://localhost:7001", "http://localhost:5085")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("PoliticaSegura");

app.MapHealthChecks("/health");

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();