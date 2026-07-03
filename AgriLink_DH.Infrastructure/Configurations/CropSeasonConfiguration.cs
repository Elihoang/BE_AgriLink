using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class CropSeasonConfiguration : IEntityTypeConfiguration<CropSeason>
{
    public void Configure(EntityTypeBuilder<CropSeason> builder)
    {
        builder.ToTable("crop_seasons");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.FarmId).HasColumnName("farm_id").IsRequired();
        builder.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
        builder.Property(x => x.StartDate).HasColumnName("start_date");
        builder.Property(x => x.EndDate).HasColumnName("end_date");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(20);
        builder.Property(x => x.CurrentStage).HasColumnName("current_stage").HasMaxLength(100);
        builder.Property(x => x.StageChangedAt).HasColumnName("stage_changed_at");
        builder.Property(x => x.StageNotes).HasColumnName("stage_notes");
        builder.Property(x => x.Note).HasColumnName("note");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
    }
}
