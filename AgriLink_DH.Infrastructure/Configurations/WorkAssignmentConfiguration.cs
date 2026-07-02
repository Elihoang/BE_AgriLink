using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class WorkAssignmentConfiguration : IEntityTypeConfiguration<WorkAssignment>
{
    public void Configure(EntityTypeBuilder<WorkAssignment> builder)
    {
        builder.ToTable("work_assignments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.LogId).HasColumnName("log_id").IsRequired();
        builder.Property(x => x.WorkerId).HasColumnName("worker_id").IsRequired();
        builder.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(20);
        builder.Property(x => x.Quantity).HasColumnName("quantity");
        builder.Property(x => x.UnitPrice).HasColumnName("unit_price");
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount");
        builder.Property(x => x.Note).HasColumnName("note");
    }
}
