using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemaPulperia.Web.Models.Entities; // Asegura este using

namespace SistemaPulperia.Web.Models.Entities
{
    [Table("RolMenus")]
    public class RolMenu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RolId { get; set; } 

        [ForeignKey("RolId")]
        // CORRECCIÓN: Cambiar ApplicationRole por NivelAcceso
        public virtual NivelAcceso Rol { get; set; } 

        public int MenuId { get; set; }

        [ForeignKey("MenuId")]
        public virtual Menu Menu { get; set; }
        
        public bool PuedeCrear { get; set; } = false;
        public bool PuedeEditar { get; set; } = false;
        public bool PuedeEliminar { get; set; } = false;
        public DateTime? FechaExpiracion { get; set; }
    }
}