using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class ArticleLikeConfiguration : IEntityTypeConfiguration<ArticleLike>
{
    public void Configure(EntityTypeBuilder<ArticleLike> builder)
    {
        builder.ToTable("article_likes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ArticleId).HasColumnName("article_id").IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}
