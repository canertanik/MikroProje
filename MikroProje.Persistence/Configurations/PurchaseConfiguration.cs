using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroProje.Domain.Entities;

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

        // Soft Delete and RowVersion are assumed to be handled in the BaseEntity configuration or globally if needed, 
        // but here we define RowVersion explicitly just in case:
        builder.HasOne(x => x.CurrentAccount)
            .WithMany()
            .HasForeignKey(x => x.CurrentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Purchase)
            .HasForeignKey(x => x.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
