using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaPulperia.Models.Entities
{
    public class Cuenta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PersonaId { get; set; }

        [ForeignKey("PersonaId")]
        public virtual Persona Persona { get; set; }

        [Required]
        [Range(0, 999999.99)]
        public decimal MontoMaximoCredito { get; set; } = 0;

        public decimal SaldoActual { get; set; } = 0;

        public DateTime FechaApertura { get; set; } = DateTime.Now;
        public DateTime? FechaVencimiento { get; set; }

        public bool EstaActiva { get; set; } = true;
        public bool EnMora => DateTime.Now > FechaVencimiento && SaldoActual > 0;

        // Relación 1 a muchos con Transacciones
        public virtual ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
    }
}