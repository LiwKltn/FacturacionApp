using Microsoft.EntityFrameworkCore;
using FacturacionApp.Models;

namespace FacturacionApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<LineaFactura> LineasFactura { get; set; }
    }
}

