using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemaPulperia.Models.Enums;

namespace SistemaPulperia.Models.Entities
{
    public class Transaccion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CuentaId { get; set; }

        [ForeignKey("CuentaId")]
        public virtual Cuenta Cuenta { get; set; }

        [Required]
        public TipoTransaccion Tipo { get; set; }

        [Required]
        public decimal Monto { get; set; }

        public string? Descripcion { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        // Trazabilidad: Quién registró el movimiento (Admin)
        [Required]
        public string UsuarioId { get; set; }
    }
}