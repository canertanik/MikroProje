using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroProje.Domain.Entities;

namespace MikroProje.Persistence.Configurations;

public class StockTransferItemConfiguration : IEntityTypeConfiguration<StockTransferItem>
{
    public void Configure(EntityTypeBuilder<StockTransferItem> builder)
    {
        builder.HasKey(sti => sti.Id);

        builder.Property(sti => sti.Quantity)
            .IsRequired();

        builder.HasOne(sti => sti.StockTransfer)
            .WithMany(st => st.Items)
            .HasForeignKey(sti => sti.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade); // If transfer is hard deleted, items can go, but soft delete will handle it

        builder.HasOne(sti => sti.Product)
            .WithMany()
            .HasForeignKey(sti => sti.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
