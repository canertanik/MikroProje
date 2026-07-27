using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroProje.Domain.Entities;

namespace MikroProje.Persistence.Configurations;

public class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.HasKey(st => st.Id);

        builder.Property(st => st.TransferNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(st => st.Description)
            .HasMaxLength(500);

        builder.Property(st => st.RowVersion)
            .IsRowVersion();

        builder.HasIndex(st => st.TransferNumber)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(st => st.SourceWarehouse)
            .WithMany()
            .HasForeignKey(st => st.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.DestinationWarehouse)
            .WithMany()
            .HasForeignKey(st => st.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
