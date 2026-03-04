using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaPulperia.Models.Entities
{
    public class HistorialContrasena
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual ApplicationUser Usuario { get; set; }

        [Required]
        public string PasswordHash { get; set; } // Guardamos el hash para comparar

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}