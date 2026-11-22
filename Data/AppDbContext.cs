using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaStockWeb.Models.Entities;

namespace SistemaStockWeb.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Categoria> Categorias { get; set; } = default!;
    public DbSet<Producto> Productos { get; set; } = default!;
    public DbSet<Deposito> Depositos { get; set; } = default!;
    public DbSet<Proveedor> Proveedores { get; set; } = default!;
    public DbSet<Cliente> Clientes { get; set; } = default!;
    public DbSet<StockDeposito> StockDepositos { get; set; } = default!;
    public DbSet<Movimiento> Movimientos { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primero se configuran las tablas propias de Identity.
        base.OnModelCreating(modelBuilder);

        // Evitar borrados en cascada: no se puede eliminar una categoría con productos
        // ni un producto con movimientos registrados.
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.idCategoria)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Movimiento>()
            .HasOne(m => m.Producto)
            .WithMany(p => p.Movimientos)
            .HasForeignKey(m => m.idProducto)
            .OnDelete(DeleteBehavior.Restrict);

        // Un producto tiene una sola fila de stock por depósito.
        modelBuilder.Entity<StockDeposito>()
            .HasIndex(s => new { s.idProducto, s.idDeposito })
            .IsUnique();

        modelBuilder.Entity<Producto>().Property(p => p.PrecioCosto).HasPrecision(18, 2);
        modelBuilder.Entity<Producto>().Property(p => p.PrecioVenta).HasPrecision(18, 2);

        // Datos iniciales.
        var fechaInicial = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { id = 1, Nombre = "Alimentos", Activo = true, CreatedAt = fechaInicial },
            new Categoria { id = 2, Nombre = "Limpieza", Activo = true, CreatedAt = fechaInicial },
            new Categoria { id = 3, Nombre = "Lacteos", Activo = true, CreatedAt = fechaInicial });

        modelBuilder.Entity<Deposito>().HasData(
            new Deposito { id = 1, Nombre = "Depósito General", Ubicacion = "Planta baja", Activo = true, CreatedAt = fechaInicial },
            new Deposito { id = 2, Nombre = "Depósito Principal", Ubicacion = "Planta alta", Activo = true, CreatedAt = fechaInicial },
            new Deposito { id = 3, Nombre = "Depósito Secundario", Ubicacion = "Planta baja", Activo = true, CreatedAt = fechaInicial });

        modelBuilder.Entity<Proveedor>().HasData(
            new Proveedor { id = 1, Nombre = "Proveedor A", Activo = true, CreatedAt = fechaInicial });

        modelBuilder.Entity<Cliente>().HasData(
            new Cliente { id = 1, Nombre = "Cliente A", Activo = true, CreatedAt = fechaInicial });
    }
}