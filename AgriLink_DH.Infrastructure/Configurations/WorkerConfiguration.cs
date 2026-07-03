using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class WorkerConfiguration : IEntityTypeConfiguration<Worker>
{
    public void Configure(EntityTypeBuilder<Worker> builder)
    {
        builder.ToTable("workers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(100);
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(x => x.WorkerType).HasColumnName("worker_type");
        builder.Property(x => x.DefaultDailyWage).HasColumnName("default_daily_wage");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
        builder.Property(x => x.MomoPhone).HasColumnName("momo_phone").HasMaxLength(20);
        builder.Property(x => x.BankAccount).HasColumnName("bank_account").HasMaxLength(50);
        builder.Property(x => x.BankName).HasColumnName("bank_name").HasMaxLength(100);
    }
}
