using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionApp.Models
{
    public class Empresa
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Nombre Empresa")]
        public string Nombre { get; set; } = "Empresa Predeterminada";

        [Required(ErrorMessage = "El CIF es obligatorio")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres")]
        [RegularExpression(@"^[A-Za-z]\d{7}[A-Za-z0-9]$", ErrorMessage = "Formato de CIF inválido")]
        [Column("Cif")] 
        [Display(Name = "CIF/NIF")]
        public string Cif { get; set; }

        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Phone(ErrorMessage = "Teléfono inválido")]
        [StringLength(15, ErrorMessage = "Máximo 15 caracteres")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; } 

        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Correo Electrónico")]
        public string? Email { get; set; }

        
    }
}