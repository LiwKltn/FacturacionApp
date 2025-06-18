using FacturacionApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace FacturacionApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets requeridos
        public DbSet<Producto> Productos { get; set; } = null!;
        public DbSet<LineaFactura> LineaFacturas { get; set; } = null!;
        public DbSet<Factura> Facturas { get; set; } = null!;
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Empresa> Empresas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones de modelos
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasIndex(p => p.Codigo).IsUnique();
                entity.Property(p => p.Precio).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<LineaFactura>(entity =>
            {
                entity.HasOne(l => l.Producto)
                    .WithMany()
                    .HasForeignKey(l => l.ProductoId);
            });
        }
    }
}