using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionApp.Models
{
    public class LineaFactura
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FacturaId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal IvaPorcentaje { get; set; }

        // Propiedad calculada
        [NotMapped]
        public decimal TotalLinea => Cantidad * PrecioUnitario;

        [ForeignKey("FacturaId")]
        public virtual Factura Factura { get; set; } = null!;

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; } = null!;
    }
}