using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroProje.Domain.Entities;

namespace MikroProje.Persistence.Configurations;

public class ProductWarehouseStockConfiguration : IEntityTypeConfiguration<ProductWarehouseStock>
{
    public void Configure(EntityTypeBuilder<ProductWarehouseStock> builder)
    {
        builder.HasKey(pws => pws.Id);

        builder.Property(pws => pws.Quantity)
            .IsRequired();

        builder.Property(pws => pws.RowVersion)
            .IsRowVersion();

        // One Product to Many ProductWarehouseStock
        builder.HasOne(pws => pws.Product)
            .WithMany(p => p.ProductWarehouseStocks)
            .HasForeignKey(pws => pws.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // One Warehouse to Many ProductWarehouseStock
        builder.HasOne(pws => pws.Warehouse)
            .WithMany(w => w.ProductWarehouseStocks)
            .HasForeignKey(pws => pws.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pws => new { pws.ProductId, pws.WarehouseId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_ProductWarehouseStocks_Quantity_Positive", "[Quantity] >= 0");
        });
    }
}
