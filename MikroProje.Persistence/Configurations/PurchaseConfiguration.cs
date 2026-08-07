using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;

namespace MikroProje.Persistence.Configurations;

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable("Purchases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Subtotal)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.VatAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.GrandTotal)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.PurchaseDate)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasDefaultValue(PurchaseStatus.Pending);

        builder.Property(x => x.ReceivedDate);

        builder.HasOne(x => x.CurrentAccount)
            .WithMany()
            .HasForeignKey(x => x.CurrentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Purchase)
            .HasForeignKey(x => x.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
