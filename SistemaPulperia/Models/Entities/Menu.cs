using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaPulperia.Web.Models.Entities 
{
    [Table("Menus")]
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } // Ej: "Bodegas"

        [StringLength(50)]
        public string? Controlador { get; set; } // Ej: "Bodegas"

        [StringLength(50)]
        public string? Accion { get; set; } // Ej: "Index"

        [StringLength(50)]
        public string? Icono { get; set; } // Ej: "bi bi-box"

        public int? PadreId { get; set; } // Null = Es un menú principal
        
        [ForeignKey("PadreId")]
        public virtual Menu? MenuPadre { get; set; }

        public int Orden { get; set; } 

        public bool Activo { get; set; } = true;

        public virtual ICollection<Menu> SubMenus { get; set; } = new List<Menu>();
    }
}