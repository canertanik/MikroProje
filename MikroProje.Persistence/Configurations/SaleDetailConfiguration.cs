using MikroProje.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MikroProje.Persistence.Configurations;

public class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
{
    public void Configure(EntityTypeBuilder<SaleDetail> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.VatRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.Discount)
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(0m);

        builder.Property(x => x.LineTotal)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Sale)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.SaleDetails)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
