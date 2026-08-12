using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Common;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;

namespace MikroProje.Persistence.Contexts;

public class MikroProjeDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public MikroProjeDbContext(DbContextOptions<MikroProjeDbContext> options, ICurrentUserService? currentUserService = null)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<CurrentAccount> CurrentAccounts => Set<CurrentAccount>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Purchase> Purchases { get; set; } = null!;
    public DbSet<PurchaseItem> PurchaseItems { get; set; } = null!;
    public DbSet<SupplierPayment> SupplierPayments { get; set; } = null!;
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<ProductWarehouseStock> ProductWarehouseStocks => Set<ProductWarehouseStock>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferItem> StockTransferItems => Set<StockTransferItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MikroProjeDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        throw new NotSupportedException("Lütfen SaveChangesAsync metodunu kullanınız.");
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        throw new NotSupportedException("Lütfen SaveChangesAsync metodunu kullanınız.");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditDrafts = HandleAuditsBeforeSave();

        if (!auditDrafts.Any())
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        var isOuterTransaction = Database.CurrentTransaction != null;

        if (isOuterTransaction)
        {
            // Flow B: Outer transaction exists. Do NOT commit/rollback here. Do not start new transaction.
            var result = await base.SaveChangesAsync(cancellationToken);

            AssignIdentitiesToDrafts(auditDrafts);

            var auditLogs = auditDrafts.Select(e => e.ToAuditLog()).ToList();
            await AuditLogs.AddRangeAsync(auditLogs, cancellationToken);
            
            // This will throw if it fails, allowing outer layer to rollback.
            await base.SaveChangesAsync(cancellationToken);

            return result;
        }
        else
        {
            // Flow A: No outer transaction. Start a transaction here and commit.
            var strategy = Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await base.SaveChangesAsync(cancellationToken);

                    AssignIdentitiesToDrafts(auditDrafts);

                    var auditLogs = auditDrafts.Select(e => e.ToAuditLog()).ToList();
                    await AuditLogs.AddRangeAsync(auditLogs, cancellationToken);
                    await base.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }
    }

    private string GetPrimaryKeyValues(EntityEntry entry)
    {
        var primaryKey = entry.Metadata.FindPrimaryKey();
        if (primaryKey == null) return "0";

        var values = new List<string>();
        foreach (var property in primaryKey.Properties)
        {
            var value = entry.Property(property.Name).CurrentValue?.ToString();
            values.Add(value ?? "0");
        }

        return string.Join("-", values);
    }

    private void AssignIdentitiesToDrafts(List<AuditEntryDraft> drafts)
    {
        foreach (var draft in drafts.Where(e => e.Action == AuditAction.Create))
        {
            if (draft.Entry != null)
            {
                draft.EntityId = GetPrimaryKeyValues(draft.Entry);
            }
        }
    }

    private List<AuditEntryDraft> HandleAuditsBeforeSave()
    {
        ChangeTracker.DetectChanges();
        var drafts = new List<AuditEntryDraft>();

        var entries = ChangeTracker.Entries()
            .Where(e => !(e.Entity is IAuditIgnore) &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        foreach (var entry in entries)
        {
            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var changedColumns = new List<string>();
            var action = AuditAction.Update;

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary) continue;

                string propertyName = property.Metadata.Name;

                if (propertyName.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("hash", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("refreshtoken", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("accesstoken", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("token", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("securitystamp", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("concurrencystamp", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("stamp", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("rowversion", StringComparison.OrdinalIgnoreCase) ||
                    property.Metadata.ClrType == typeof(byte[]))
                {
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        action = AuditAction.Create;
                        newValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        action = AuditAction.Delete;
                        oldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            if (propertyName == "IsDeleted" && property.CurrentValue?.ToString() == "True")
                            {
                                action = AuditAction.SoftDelete;
                            }
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                            changedColumns.Add(propertyName);
                        }
                        break;
                }
            }
            
            // Eğer modified entity ise ancak hiçbir kolon değişmemişse veya hepsi hassas ise kaydetme
            if ((action == AuditAction.Update || action == AuditAction.SoftDelete) && changedColumns.Count == 0)
            {
                continue;
            }

            var entityId = "0";
            if (action != AuditAction.Create)
            {
                entityId = GetPrimaryKeyValues(entry);
            }

            var draft = new AuditEntryDraft
            {
                Entry = action == AuditAction.Create ? entry : null,
                EntityName = entry.Entity.GetType().Name,
                EntityId = entityId,
                Action = action,
                OldValuesJson = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues),
                NewValuesJson = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues),
                ChangedColumnsJson = changedColumns.Count == 0 ? null : JsonSerializer.Serialize(changedColumns),
                UserId = _currentUserService?.UserId ?? "System",
                Username = _currentUserService?.Username ?? "Anonymous",
                IpAddress = _currentUserService?.IpAddress,
                RequestPath = _currentUserService?.RequestPath,
                CorrelationId = _currentUserService?.CorrelationId,
                CreatedDate = DateTime.UtcNow
            };

            drafts.Add(draft);
        }

        return drafts;
    }
}

public class AuditEntryDraft
{
    public EntityEntry? Entry { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public string? ChangedColumnsJson { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? RequestPath { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime CreatedDate { get; set; }

    public AuditLog ToAuditLog()
    {
        return new AuditLog
        {
            EntityName = EntityName,
            EntityId = EntityId,
            Action = Action,
            OldValues = OldValuesJson,
            NewValues = NewValuesJson,
            ChangedColumns = ChangedColumnsJson,
            UserId = UserId,
            Username = Username,
            IpAddress = IpAddress ?? string.Empty,
            RequestPath = RequestPath ?? string.Empty,
            CorrelationId = CorrelationId,
            CreatedDate = CreatedDate
        };
    }
}


