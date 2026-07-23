using MikroProje.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MikroProje.Persistence.Contexts;

public class MikroProjeDbContext : DbContext
{
    public MikroProjeDbContext(DbContextOptions<MikroProjeDbContext> options)
        : base(options)
    {
    }

    public DbSet<CurrentAccount> CurrentAccounts => Set<CurrentAccount>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<Sale> Sales => Set<Sale>();

    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MikroProjeDbContext).Assembly);
    }
}
