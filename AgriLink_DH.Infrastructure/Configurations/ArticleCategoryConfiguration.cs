using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class ArticleCategoryConfiguration : IEntityTypeConfiguration<ArticleCategory>
{
    public void Configure(EntityTypeBuilder<ArticleCategory> builder)
    {
        builder.ToTable("article_categories");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).HasColumnName("code");
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(x => x.Icon).HasColumnName("icon").HasMaxLength(50);
        builder.Property(x => x.Color).HasColumnName("color").HasMaxLength(20);
        builder.Property(x => x.DisplayOrder).HasColumnName("display_order");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
