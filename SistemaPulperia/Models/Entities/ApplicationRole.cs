using Microsoft.AspNetCore.Identity;

namespace SistemaPulperia.Web.Models.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public bool Activo { get; set; } = true;
        public string? Descripcion { get; set; }

        // Constructor vacío necesario para EF Core
        public ApplicationRole() : base() { }

        // Constructor para crear roles solo con nombre
        public ApplicationRole(string roleName) : base(roleName) { }

        // Constructor completo
        public ApplicationRole(string roleName, string descripcion) : base(roleName)
        {
            Descripcion = descripcion;
        }
    }
}