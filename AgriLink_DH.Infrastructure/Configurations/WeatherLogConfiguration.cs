using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class WeatherLogConfiguration : IEntityTypeConfiguration<WeatherLog>
{
    public void Configure(EntityTypeBuilder<WeatherLog> builder)
    {
        builder.ToTable("weather_logs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.FarmId).HasColumnName("farm_id").IsRequired();
        builder.Property(x => x.LogDate).HasColumnName("log_date");
        builder.Property(x => x.Condition).HasColumnName("condition").HasMaxLength(50);
        builder.Property(x => x.RainfallMm).HasColumnName("rainfall_mm");
        builder.Property(x => x.Note).HasColumnName("note");
    }
}
