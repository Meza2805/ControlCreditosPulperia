using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaPulperia.Models;
using SistemaPulperia.Models.Entities;
using SistemaPulperia.Web.Models.Entities;

namespace SistemaPulperia.Data
{
    // 1. CORRECCIÓN CLAVE: Se añade NivelAcceso a la firma de la clase para que EF lo incluya en el modelo
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, NivelAcceso, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tablas de negocio
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Cuenta> Cuentas { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<RolMenu> RolMenus { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Es vital mantener esta línea para la configuración de Identity
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Persona>()
                .HasIndex(p => p.Cedula)
                .IsUnique(); // Esto crea una restricción UNIQUE en SQL Server
            // 2. REGISTRO FORMAL DE LA ENTIDAD: Esto soluciona el error de "not included in the model"
            modelBuilder.Entity<NivelAcceso>(entity =>
            {
                entity.ToTable("AspNetRoles"); // Mapea a la tabla física de roles
                entity.HasBaseType((Type)null); // Desactiva la herencia TPH para evitar discriminadores vacíos
            });

            // Configuración Fluent API para relación Persona -> Cuentas (1:N)
            modelBuilder.Entity<Persona>()
                .HasMany(p => p.Cuentas) // Una persona tiene muchas cuentas
                .WithOne(c => c.Persona) // Cada cuenta pertenece a una persona
                .HasForeignKey(c => c.PersonaId) // La llave foránea está en la tabla Cuenta
                .OnDelete(DeleteBehavior.Cascade); // Si se borra la persona, se borran sus cuentas
            // Configuración de precisión para montos decimales
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            // =============================================================
            // SEED DATA: MÓDULO ADMINISTRACIÓN
            // =============================================================

            // 1. Menú Principal: Usuarios
            modelBuilder.Entity<Menu>().HasData(
                new Menu { Id = 60, Nombre = "Usuarios", Icono = "bi bi-people-fill", Orden = 9, Activo = true }
            );

            // 2. Submenús de Usuarios
            modelBuilder.Entity<Menu>().HasData(
                new Menu { Id = 61, PadreId = 60, Nombre = "Nuevo Usuario", Controlador = "Account", Accion = "Registrar", Icono = "bi bi-person-plus", Orden = 1, Activo = true },
                new Menu { Id = 62, PadreId = 60, Nombre = "Restablecer Contraseña", Controlador = "Account", Accion = "ResetPassword", Icono = "bi bi-key-fill", Orden = 2, Activo = true },
                new Menu { Id = 63, PadreId = 60, Nombre = "Lista de Usuarios", Controlador = "Account", Accion = "Index", Icono = "bi bi-person-lines-fill", Orden = 3, Activo = true }
            );

            // 3. Menú Principal: Configuración de Seguridad
            modelBuilder.Entity<Menu>().HasData(
                new Menu { Id = 70, Nombre = "Seguridad y Niveles de Acceso", Icono = "bi bi-shield-check", Orden = 10, Activo = true }
            );

            // 4. Submenús de Seguridad
            modelBuilder.Entity<Menu>().HasData(
                new Menu { Id = 71, PadreId = 70, Nombre = "Administrar Niveles de Acceso", Controlador = "Roles", Accion = "Index", Icono = "bi bi-lock-fill", Orden = 1, Activo = true },
                new Menu { Id = 72, PadreId = 70, Nombre = "Permisos de Menú", Controlador = "Roles", Accion = "PermisosMenus", Icono = "bi bi-list-check", Orden = 2, Activo = true }
            );
        }
    }
}