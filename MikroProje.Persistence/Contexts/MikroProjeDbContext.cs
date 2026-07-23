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
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<Sale> Sales => Set<Sale>();

    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();

    public DbSet<Payment> Payments { get; set; } = null!;

    public DbSet<Purchase> Purchases { get; set; } = null!;

    public DbSet<PurchaseItem> PurchaseItems { get; set; } = null!;

    public DbSet<SupplierPayment> SupplierPayments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MikroProjeDbContext).Assembly);
    }
}
