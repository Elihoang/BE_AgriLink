using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("articles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.CategoryId).HasColumnName("category_id").IsRequired();
        builder.Property(x => x.AuthorId).HasColumnName("author_id").IsRequired();
        builder.Property(x => x.Title).HasColumnName("title").IsRequired().HasMaxLength(300);
        builder.Property(x => x.Slug).HasColumnName("slug").IsRequired().HasMaxLength(300);
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(x => x.Content).HasColumnName("content");
        builder.Property(x => x.ThumbnailUrl).HasColumnName("thumbnail_url").HasMaxLength(500);
        builder.Property(x => x.Tags).HasColumnName("tags").HasMaxLength(500);
        builder.Property(x => x.Hashtags).HasColumnName("hashtags").HasMaxLength(500);
        builder.Property(x => x.ReadTime).HasColumnName("read_time");
        builder.Property(x => x.AudioUrl).HasColumnName("audio_url").HasMaxLength(500);
        builder.Property(x => x.AudioDuration).HasColumnName("audio_duration");
        builder.Property(x => x.VideoUrl).HasColumnName("video_url").HasMaxLength(500);
        builder.Property(x => x.ViewCount).HasColumnName("view_count");
        builder.Property(x => x.LikeCount).HasColumnName("like_count");
        builder.Property(x => x.CommentCount).HasColumnName("comment_count");
        builder.Property(x => x.ShareCount).HasColumnName("share_count");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.IsFeatured).HasColumnName("is_featured");
        builder.Property(x => x.AllowComments).HasColumnName("allow_comments");
        builder.Property(x => x.PublishedAt).HasColumnName("published_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.SeoMetadata).HasColumnName("seo_metadata").HasMaxLength(2000);
    }
}
