using Microsoft.EntityFrameworkCore;
using ReservaCancha.Models;

namespace ReservaCancha.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Cancha> Canchas => Set<Cancha>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            
            modelBuilder.Entity<Cancha>()
                .Property(c => c.Precio)
                .HasColumnType("decimal(10,2)");

            
            modelBuilder.Entity<Cancha>()
                .Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Cancha>()
                .Property(c => c.Tipo)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Cancha>()
                .Property(c => c.Descripcion)
                .HasMaxLength(300);

            
            modelBuilder.Entity<Cancha>().HasData(
                new Cancha
                {
                    Id = 1,
                    Nombre = "Cancha Norte",
                    Tipo = "Futbol",
                    Precio = 150,
                    Descripcion = "Cancha de grama sintetica con iluminacion nocturna.",
                    Disponible = true
                },
                new Cancha
                {
                    Id = 2,
                    Nombre = "Cancha Sur",
                    Tipo = "Basquetbol",
                    Precio = 100,
                    Descripcion = "Cancha techada con piso de madera.",
                    Disponible = true
                },
                new Cancha
                {
                    Id = 3,
                    Nombre = "Cancha Este",
                    Tipo = "Voleibol",
                    Precio = 80,
                    Descripcion = "Cancha de arena al aire libre.",
                    Disponible = true
                }
            );
        }
    }
}