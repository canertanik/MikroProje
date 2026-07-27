using MikroProje.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MikroProje.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.VatAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.GrandTotal)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.CurrentAccount)
            .WithMany(x => x.Sales)
            .HasForeignKey(x => x.CurrentAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CurrentAccountId);
        builder.HasIndex(x => x.SaleDate);
        builder.HasIndex(x => x.IsDeleted);
    }
}
