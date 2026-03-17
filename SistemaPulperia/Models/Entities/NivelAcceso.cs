using Microsoft.AspNetCore.Identity;

namespace SistemaPulperia.Web.Models.Entities
{
    public class NivelAcceso : IdentityRole
    {
        // 1. Constructor vacío: Indispensable para que EF Core no falle al leer de la BD
        public NivelAcceso() : base() { }

        // 2. Constructor para creación: 
        // CAMBIO CLAVE: Cambia el nombre del parámetro de 'roleName' a 'name' 
        // para que coincida con la propiedad 'Name' de IdentityRole.
        public NivelAcceso(string name) : base(name) 
        {
            this.Name = name;
        }

        // Tus propiedades adicionales (si las tienes)
        public bool Activo { get; set; }
        public string? Descripcion { get; set; }
    }
}