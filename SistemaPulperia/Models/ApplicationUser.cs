using Microsoft.AspNetCore.Identity;

namespace SistemaPulperia.Models
{
public class ApplicationUser : IdentityUser
    {
        public string? NombreCompleto { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public string? CreadoPor { get; set; }
        public bool Estado { get; set; }
        public DateTime? FechaExpiracionAcceso { get; set; }
        public string? UltimaIP { get; set; }
        public string? SesionActivaId { get; set; }
        

        //  NUEVA RELACIÓN: Un Usuario puede tener muchas asignaciones de Área
        //public virtual ICollection<UsuarioArea> UsuarioAreas { get; set; }

        // Constructor para inicializar la lista y evitar NullReferenceException
        // public ApplicationUser()
        // {
        //     UsuarioAreas = new HashSet<UsuarioArea>();
        // }
    }
}