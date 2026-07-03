using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class MaterialUsageConfiguration : IEntityTypeConfiguration<MaterialUsage>
{
    public void Configure(EntityTypeBuilder<MaterialUsage> builder)
    {
        builder.ToTable("material_usages");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SeasonId).HasColumnName("season_id").IsRequired();
        builder.Property(x => x.UsageDate).HasColumnName("usage_date");
        builder.Property(x => x.MaterialName).HasColumnName("material_name").HasMaxLength(150);
        builder.Property(x => x.Quantity).HasColumnName("quantity");
        builder.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(20);
        builder.Property(x => x.UnitPrice).HasColumnName("unit_price");
        builder.Property(x => x.TotalCost).HasColumnName("total_cost");
        builder.Property(x => x.Note).HasColumnName("note");
        builder.Property(x => x.MaterialId).HasColumnName("material_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(x => x.CropSeason).WithMany(c => c.MaterialUsages).HasForeignKey(x => x.SeasonId);
    }
}
