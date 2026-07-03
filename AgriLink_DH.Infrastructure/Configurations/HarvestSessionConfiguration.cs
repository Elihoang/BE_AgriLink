using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class HarvestSessionConfiguration : IEntityTypeConfiguration<HarvestSession>
{
    public void Configure(EntityTypeBuilder<HarvestSession> builder)
    {
        builder.ToTable("harvest_sessions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SeasonId).HasColumnName("season_id").IsRequired();
        builder.Property(x => x.HarvestDate).HasColumnName("harvest_date");
        builder.Property(x => x.TotalBags).HasColumnName("total_bags");
        builder.Property(x => x.TotalWeight).HasColumnName("total_weight");
        builder.Property(x => x.StorageLocation).HasColumnName("storage_location").HasMaxLength(50);
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(x => x.CropSeason).WithMany(c => c.HarvestSessions).HasForeignKey(x => x.SeasonId);
    }
}
