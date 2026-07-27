using Microsoft.EntityFrameworkCore;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<CurrentAccount> CurrentAccounts { get; }
    DbSet<Sale> Sales { get; }
    DbSet<Purchase> Purchases { get; }
    DbSet<AuditLog> AuditLogs { get; }
    // Other DbSets can be added as needed for read-only CQRS queries
}
