using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Username).IsRequired().HasColumnName("username").HasMaxLength(50);
        builder.Property(x => x.Email).IsRequired().HasColumnName("email").HasMaxLength(100);
        builder.Property(x => x.PasswordHash).IsRequired().HasColumnName("password_hash");
        builder.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(100);
        builder.Property(x => x.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20);
        builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(x => x.Role).HasColumnName("role");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
