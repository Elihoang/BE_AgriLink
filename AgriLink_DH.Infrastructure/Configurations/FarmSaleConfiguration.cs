using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class FarmSaleConfiguration : IEntityTypeConfiguration<FarmSale>
{
    public void Configure(EntityTypeBuilder<FarmSale> builder)
    {
        builder.ToTable("farm_sales");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SeasonId).HasColumnName("season_id").IsRequired();
        builder.Property(x => x.SaleDate).HasColumnName("sale_date");
        builder.Property(x => x.BuyerName).HasColumnName("buyer_name").HasMaxLength(100);
        builder.Property(x => x.QuantitySold).HasColumnName("quantity_sold");
        builder.Property(x => x.PricePerKg).HasColumnName("price_per_kg");
        builder.Property(x => x.TotalRevenue).HasColumnName("total_revenue");
        builder.Property(x => x.Note).HasColumnName("note");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(x => x.CropSeason).WithMany(c => c.FarmSales).HasForeignKey(x => x.SeasonId);
    }
}
