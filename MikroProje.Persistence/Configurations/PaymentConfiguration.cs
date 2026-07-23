using MikroProje.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MikroProje.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.PaymentDate)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.PaymentMethod)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(x => x.CurrentAccount)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.CurrentAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.CurrentAccountId);
        builder.HasIndex(x => x.PaymentDate);
        builder.HasIndex(x => x.IsDeleted);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Payments_Amount_Positive", "[Amount] > 0");
        });
    }
}
