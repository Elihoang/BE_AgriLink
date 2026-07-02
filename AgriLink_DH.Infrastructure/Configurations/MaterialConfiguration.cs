using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("materials");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(50);
        builder.Property(x => x.QuantityInStock).HasColumnName("quantity_in_stock").HasColumnType("decimal(18,2)");
        builder.Property(x => x.CostPerUnit).HasColumnName("cost_per_unit").HasColumnType("decimal(18,2)");
        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(500);
        builder.Property(x => x.ImageUrl).HasColumnName("image_url");
        builder.Property(x => x.MaterialType).HasColumnName("material_type");
        builder.Property(x => x.ExpiryDate).HasColumnName("expiry_date");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerUserId);
    }
}
