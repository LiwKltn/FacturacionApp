using System.Collections.Generic;
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
        public string Numero { get; set; } = null!;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Emisión")]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [ForeignKey("Cliente")]
        [Display(Name = "Cliente")]
        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        public int ClienteId { get; set; }

        public Cliente Cliente { get; set; } = null!;

        [ForeignKey("Empresa")]
        [Display(Name = "Empresa Emisora")]
        [Required(ErrorMessage = "Debe seleccionar una empresa")]
        public int EmpresaId { get; set; }

        public Empresa Empresa { get; set; } = null!;

        public List<LineaFactura> Lineas { get; set; } = new List<LineaFactura>();

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
    }
}