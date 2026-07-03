using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class WorkerAdvanceConfiguration : IEntityTypeConfiguration<WorkerAdvance>
{
    public void Configure(EntityTypeBuilder<WorkerAdvance> builder)
    {
        builder.ToTable("worker_advances");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.WorkerId).HasColumnName("worker_id").IsRequired();
        builder.Property(x => x.SeasonId).HasColumnName("season_id").IsRequired();
        builder.Property(x => x.Amount).HasColumnName("amount").IsRequired();
        builder.Property(x => x.AdvanceDate).HasColumnName("advance_date");
        builder.Property(x => x.IsDeducted).HasColumnName("is_deducted");
        builder.Property(x => x.Note).HasColumnName("note");

        builder.HasOne(x => x.CropSeason).WithMany(c => c.WorkerAdvances).HasForeignKey(x => x.SeasonId);
    }
}
