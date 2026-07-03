using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class ArticleCommentConfiguration : IEntityTypeConfiguration<ArticleComment>
{
    public void Configure(EntityTypeBuilder<ArticleComment> builder)
    {
        builder.ToTable("article_comments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ArticleId).HasColumnName("article_id").IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.ParentCommentId).HasColumnName("parent_comment_id");
        builder.Property(x => x.Content).HasColumnName("content").IsRequired().HasMaxLength(2000);
        builder.Property(x => x.LikeCount).HasColumnName("like_count");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
