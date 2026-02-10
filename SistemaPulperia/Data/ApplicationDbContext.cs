using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaPulperia.Models;
using SistemaPulperia.Models.Entities;

namespace SistemaPulperia.Data
{
    // Heredamos de IdentityDbContext usando nuestra clase personalizada de usuario
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tablas de la base de datos
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Cuenta> Cuentas { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Es vital mantener esta línea para la configuración de Identity
            base.OnModelCreating(modelBuilder);

            // Configuración Fluent API: Relación 1 a 1 entre Persona y Cuenta
            modelBuilder.Entity<Persona>()
                .HasOne(p => p.Cuenta)
                .WithOne(c => c.Persona)
                .HasForeignKey<Cuenta>(c => c.PersonaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de precisión para montos decimales
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }
        }
    }
}