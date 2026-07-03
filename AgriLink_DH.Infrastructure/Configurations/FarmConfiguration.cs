using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class FarmConfiguration : IEntityTypeConfiguration<Farm>
{
    public void Configure(EntityTypeBuilder<Farm> builder)
    {
        builder.ToTable("farms");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
        builder.Property(x => x.AreaSize).HasColumnName("area_size");
        builder.Property(x => x.AddressGps).HasColumnName("address_gps").HasMaxLength(255);
        builder.Property(x => x.Latitude).HasColumnName("latitude");
        builder.Property(x => x.Longitude).HasColumnName("longitude");
        builder.Property(x => x.GoogleMapsUrl).HasColumnName("google_maps_url").HasMaxLength(500);
        builder.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
    }
}
