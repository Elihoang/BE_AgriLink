using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Unit)
            .HasColumnName("unit")
            .HasMaxLength(20);

        builder.Property(p => p.Code)
            .HasColumnName("code")
            .HasMaxLength(20);

        builder.Property(p => p.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(p => p.Code).IsUnique();

        // Seed data
        builder.HasData(
            new 
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "Cà phê Robusta",
                Unit = "kg",
                Code = "CF_ROBUSTA"
            },
            new 
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "Hồ Tiêu",
                Unit = "kg",
                Code = "PEPPER"
            },
            new 
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "Sầu Riêng",
                Unit = "kg",
                Code = "DURIAN"
            }
        );
    }
}
