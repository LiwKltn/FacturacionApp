using System.ComponentModel.DataAnnotations;

namespace FacturacionApp.Models
{
    public class Empresa
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El NIF es obligatorio")]
        public string NIF { get; set; }

        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }

        [Display(Name = "Logo")]
        public string LogoUrl { get; set; }
    }
}