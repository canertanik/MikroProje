using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroProje.Domain.Entities;

namespace MikroProje.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.EntityName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(200).IsRequired(false);
        builder.Property(x => x.UserId).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.Username).HasMaxLength(200).IsRequired(false);
        builder.Property(x => x.IpAddress).HasMaxLength(64).IsRequired(false);
        builder.Property(x => x.RequestPath).HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired(false);

        builder.HasIndex(x => x.CreatedDate);
        builder.HasIndex(x => new { x.EntityName, x.EntityId, x.CreatedDate });
        builder.HasIndex(x => new { x.UserId, x.CreatedDate });
        builder.HasIndex(x => new { x.Action, x.CreatedDate });
    }
}
