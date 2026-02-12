using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaPulperia.Web.Models.Entities
{
    [Table("RolMenus")]
    public class RolMenu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RolId { get; set; } // ID del Rol (es un string en Identity)

        [ForeignKey("RolId")]
        public virtual ApplicationRole Rol { get; set; } 

        public int MenuId { get; set; }

        [ForeignKey("MenuId")]
        public virtual Menu Menu { get; set; }
        
        // Aquí podrías agregar permisos específicos a futuro
        public bool PuedeCrear { get; set; } = false;
        public bool PuedeEditar { get; set; } = false;
        public bool PuedeEliminar { get; set; } = false;
        public DateTime? FechaExpiracion { get; set; }
    }
}