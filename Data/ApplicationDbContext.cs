using FacturacionApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FacturacionApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Factura> Facturas { get; set; }
        public DbSet<LineaFactura> LineaFacturas { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Empresa> Empresas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Factura
            modelBuilder.Entity<Factura>(entity =>
            {
                entity.HasIndex(f => f.Numero).IsUnique();
                entity.Property(f => f.Numero).HasMaxLength(20).IsRequired();
                entity.Property(f => f.Fecha).IsRequired();

                entity.HasOne(f => f.Cliente)
                    .WithMany()
                    .HasForeignKey(f => f.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Empresa)
                    .WithMany()
                    .HasForeignKey(f => f.EmpresaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //LineaFactura
            modelBuilder.Entity<LineaFactura>(entity =>
            {
                entity.Property(l => l.Cantidad).IsRequired();
                entity.Property(l => l.PrecioUnitario)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                entity.Property(l => l.IvaPorcentaje)
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();

                entity.HasOne(l => l.Factura)
                    .WithMany(f => f.Lineas)
                    .HasForeignKey(l => l.FacturaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.Producto)
                    .WithMany()
                    .HasForeignKey(l => l.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración para reintentos de ejecución
            //modelBuilder.UseSqlServerRetryOnFailure();
        }
    }
}