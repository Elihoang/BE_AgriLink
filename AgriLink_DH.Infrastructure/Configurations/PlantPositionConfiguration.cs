using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class PlantPositionConfiguration : IEntityTypeConfiguration<PlantPosition>
{
    public void Configure(EntityTypeBuilder<PlantPosition> builder)
    {
        builder.ToTable("plant_positions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.FarmId).IsRequired().HasColumnName("farm_id");
        builder.Property(x => x.SeasonId).HasColumnName("season_id");
        builder.Property(x => x.RowNumber).HasColumnName("row_number");
        builder.Property(x => x.ColumnNumber).HasColumnName("column_number");
        builder.Property(x => x.ProductId).IsRequired().HasColumnName("product_id");
        builder.Property(x => x.PlantDate).HasColumnName("plant_date");
        builder.Property(x => x.HealthStatus).HasColumnName("health_status");
        builder.Property(x => x.EstimatedYield).HasColumnName("estimated_yield");
        builder.Property(x => x.Note).HasColumnName("note");

        builder.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.SeasonId);
    }
}
