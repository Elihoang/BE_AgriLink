using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class MarketPriceHistoryConfiguration : IEntityTypeConfiguration<MarketPriceHistory>
{
    public void Configure(EntityTypeBuilder<MarketPriceHistory> builder)
    {
        builder.ToTable("market_price_history");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(x => x.Region).HasColumnName("region").HasMaxLength(50);
        builder.Property(x => x.RegionCode).HasColumnName("region_code").HasMaxLength(20);
        builder.Property(x => x.Price).HasColumnName("price");
        builder.Property(x => x.Change).HasColumnName("change");
        builder.Property(x => x.ChangePercent).HasColumnName("change_percent");
        builder.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(20);
        builder.Property(x => x.RecordedDate).HasColumnName("recorded_date");
        builder.Property(x => x.Source).HasColumnName("source").HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(500);
    }
}
