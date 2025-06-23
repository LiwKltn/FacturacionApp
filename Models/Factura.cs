using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionApp.Models
{
    public class Factura
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Numero { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int EmpresaId { get; set; }

        // Propiedades calculadas
        [NotMapped] // No se guarda en la base de datos
        public decimal BaseImponible => Lineas?.Sum(l => l.Cantidad * l.PrecioUnitario) ?? 0;

        [NotMapped]
        public decimal TotalIva => Lineas?.Sum(l => l.Cantidad * l.PrecioUnitario * (l.IvaPorcentaje / 100)) ?? 0;

        [NotMapped]
        public decimal Total => BaseImponible + TotalIva;

        // Relaciones
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; } = null!;

        [ForeignKey("EmpresaId")]
        public virtual Empresa Empresa { get; set; } = null!;

        public virtual ICollection<LineaFactura> Lineas { get; set; } = new List<LineaFactura>();
    }
}