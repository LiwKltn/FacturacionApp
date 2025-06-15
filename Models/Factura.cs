using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionApp.Models
{
    public class Factura
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El número de factura es obligatorio")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "El número debe tener entre 3 y 20 caracteres")]
        [Display(Name = "Número de Factura")]
        public required string Numero { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Emisión")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Fecha { get; set; } = DateTime.Today;

        // Relación con Cliente
        [ForeignKey("Cliente")]
        [Display(Name = "Cliente")]
        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        public required int ClienteId { get; set; }

        public required Cliente Cliente { get; set; }

        // Relación con Empresa
        [ForeignKey("Empresa")]
        [Display(Name = "Empresa Emisora")]
        [Required(ErrorMessage = "Debe seleccionar una empresa")]
        public required int EmpresaId { get; set; }

        public required Empresa Empresa { get; set; }

       
        // Para validar las líneas individualmente
        public List<LineaFactura> Lineas { get; set; } = new List<LineaFactura>();

        // Propiedad calculada (no se mapea a la base de datos)
        [NotMapped]
        [Display(Name = "Base Imponible")]
        [DataType(DataType.Currency)]
        public decimal BaseImponible => Lineas.Sum(l => l.Cantidad * l.PrecioUnitario);

        [NotMapped]
        [Display(Name = "Total IVA")]
        [DataType(DataType.Currency)]
        public decimal TotalIva => Lineas.Sum(l => l.Cantidad * l.PrecioUnitario * (l.IvaPorcentaje / 100));

        [NotMapped]
        [Display(Name = "Total Factura")]
        [DataType(DataType.Currency)]
        public decimal Total => BaseImponible + TotalIva;

        // Método para validación personalizada
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Lineas == null || !Lineas.Any())
            {
                yield return new ValidationResult(
                    "Debe incluir al menos una línea de factura",
                    new[] { nameof(Lineas) });
            }

            if (Fecha > DateTime.Today.AddDays(30))
            {
                yield return new ValidationResult(
                    "La fecha no puede ser más de 30 días en el futuro",
                    new[] { nameof(Fecha) });
            }
        }
    }
}