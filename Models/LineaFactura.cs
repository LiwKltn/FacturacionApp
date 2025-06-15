using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionApp.Models
{
    public class LineaFactura
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio")]
        [StringLength(100, MinimumLength = 3)]
        public required string Concepto { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 0")]
        public decimal Cantidad { get; set; } = 1;

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que 0")]
        [DataType(DataType.Currency)]
        public decimal PrecioUnitario { get; set; }

        [Range(0, 100, ErrorMessage = "El IVA debe estar entre 0% y 100%")]
        [Display(Name = "% IVA")]
        public decimal IvaPorcentaje { get; set; } = 21;

        [NotMapped]
        [Display(Name = "Importe Total")]
        [DataType(DataType.Currency)]
        public decimal ImporteTotal => Cantidad * PrecioUnitario * (1 + IvaPorcentaje / 100);

        // Relación con Factura
        public int FacturaId { get; set; }
        public Factura Factura { get; set; }
    }
}