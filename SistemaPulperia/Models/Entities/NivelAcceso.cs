using Microsoft.AspNetCore.Identity;

namespace SistemaPulperia.Web.Models.Entities
{
    // Esta es la clase que el DbInitializer no encuentra
    public class NivelAcceso : IdentityRole
    {
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
    }
}