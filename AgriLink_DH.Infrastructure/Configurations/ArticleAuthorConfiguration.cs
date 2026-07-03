using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class ArticleAuthorConfiguration : IEntityTypeConfiguration<ArticleAuthor>
{
    public void Configure(EntityTypeBuilder<ArticleAuthor> builder)
    {
        builder.ToTable("article_authors");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(50);
        builder.Property(x => x.Organization).HasColumnName("organization").HasMaxLength(200);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(500);
        builder.Property(x => x.Bio).HasColumnName("bio").HasMaxLength(1000);
        builder.Property(x => x.IsVerified).HasColumnName("is_verified");
        builder.Property(x => x.SocialLinks).HasColumnName("social_links").HasMaxLength(1000);
        builder.Property(x => x.Specialties).HasColumnName("specialties").HasMaxLength(500);
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
