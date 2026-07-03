using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class DailyWorkLogConfiguration : IEntityTypeConfiguration<DailyWorkLog>
{
    public void Configure(EntityTypeBuilder<DailyWorkLog> builder)
    {
        builder.ToTable("daily_work_logs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SeasonId).HasColumnName("season_id").IsRequired();
        builder.Property(x => x.WorkDate).HasColumnName("work_date");
        builder.Property(x => x.TaskTypeId).HasColumnName("task_type_id");
        builder.Property(x => x.Note).HasColumnName("note");
        builder.Property(x => x.TotalCost).HasColumnName("total_cost");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(x => x.CropSeason).WithMany(c => c.DailyWorkLogs).HasForeignKey(x => x.SeasonId);
    }
}
