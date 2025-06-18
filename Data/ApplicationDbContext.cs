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

        //protected override void OnModelCreating(ModelBuilder modelBuilder);
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de la relación Factura-Líneas
            modelBuilder.Entity<Factura>()
                .HasMany(f => f.Lineas)
                .WithOne(l => l.Factura)
                .HasForeignKey(l => l.FacturaId)
                .OnDelete(DeleteBehavior.Cascade); // Elimina líneas al borrar factura

            // Configuración de propiedades requeridas
            modelBuilder.Entity<Factura>()
                .Property(f => f.Numero)
                .IsRequired()
                .HasMaxLength(20);

            // Asegurar que las relaciones sean requeridas
            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Cliente)
                .WithMany()
                .HasForeignKey(f => f.ClienteId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Empresa)
                .WithMany()
                .HasForeignKey(f => f.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        }
    }
}

