using MikroProje.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MikroProje.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Barcode)
            .HasMaxLength(100);

        builder.HasIndex(x => x.Barcode)
            .IsUnique()
            .HasFilter("[Barcode] IS NOT NULL");

        builder.Property(x => x.PurchasePrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.SalePrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.VatRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.StockQuantity)
            .HasDefaultValue(0);

        builder.Property(x => x.CriticalStockQuantity)
            .HasDefaultValue(0);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Products_StockQuantity_NonNegative", "[StockQuantity] >= 0");
            tableBuilder.HasCheckConstraint("CK_Products_CriticalStockQuantity_NonNegative", "[CriticalStockQuantity] >= 0");
        });

        builder.HasMany(x => x.StockMovements)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
