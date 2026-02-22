using System.ComponentModel.DataAnnotations;

namespace SistemaPulperia.Models.Entities
{
    public class Persona
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El primer nombre es obligatorio")]
        [StringLength(50)]
        public string PrimerNombre { get; set; } = default!;

        [StringLength(50)]
        public string? SegundoNombre { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio")]
        [StringLength(50)]
        public string PrimerApellido { get; set; } = default!;

        [StringLength(50)]
        public string? SegundoApellido { get; set; }

        [StringLength(16)] // Formato 000-000000-0000X
        public string? Cedula { get; set; }

        [EmailAddress(ErrorMessage = "Por favor, ingrese una dirección de correo válida")]
        [StringLength(100)]
        public string? EmailContacto { get; set; }

        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Cambia esto para que sea opcional y no bloquee el modelo
        public virtual ICollection<Cuenta>? Cuentas { get; set; } = new List<Cuenta>();
    }
}