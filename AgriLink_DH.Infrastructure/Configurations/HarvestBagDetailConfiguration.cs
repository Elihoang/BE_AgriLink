using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class HarvestBagDetailConfiguration : IEntityTypeConfiguration<HarvestBagDetail>
{
    public void Configure(EntityTypeBuilder<HarvestBagDetail> builder)
    {
        builder.ToTable("harvest_bag_details");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SessionId).HasColumnName("session_id").IsRequired();
        builder.Property(x => x.BagIndex).HasColumnName("bag_index");
        builder.Property(x => x.GrossWeight).HasColumnName("gross_weight");
        builder.Property(x => x.Deduction).HasColumnName("deduction");
        builder.Property(x => x.NetWeight).HasColumnName("net_weight");
        builder.Property(x => x.IsAutoWeighed).HasColumnName("is_auto_weighed");
        builder.Property(x => x.ScaleDeviceId).HasColumnName("scale_device_id").HasMaxLength(100);
        builder.Property(x => x.IsDraft).HasColumnName("is_draft");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
    }
}
