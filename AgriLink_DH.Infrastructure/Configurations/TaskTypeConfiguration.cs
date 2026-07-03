using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class TaskTypeConfiguration : IEntityTypeConfiguration<TaskType>
{
    public void Configure(EntityTypeBuilder<TaskType> builder)
    {
        builder.ToTable("task_types");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.FarmId).HasColumnName("farm_id");
        builder.Property(x => x.IsSystem).HasColumnName("is_system");
        builder.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
        builder.Property(x => x.DefaultUnit).HasColumnName("default_unit").HasMaxLength(20);
        builder.Property(x => x.DefaultPrice).HasColumnName("default_price");
    }
}
