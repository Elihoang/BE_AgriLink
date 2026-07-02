using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class SalaryPaymentConfiguration : IEntityTypeConfiguration<SalaryPayment>
{
    public void Configure(EntityTypeBuilder<SalaryPayment> builder)
    {
        builder.ToTable("salary_payments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.WorkerId).HasColumnName("worker_id").IsRequired();
        builder.Property(x => x.PeriodStart).HasColumnName("period_start");
        builder.Property(x => x.PeriodEnd).HasColumnName("period_end");
        builder.Property(x => x.GrossSalary).HasColumnName("gross_salary");
        builder.Property(x => x.TotalAdvance).HasColumnName("total_advance");
        builder.Property(x => x.NetSalary).HasColumnName("net_salary");
        builder.Property(x => x.MomoOrderId).HasColumnName("momo_order_id").HasMaxLength(100);
        builder.Property(x => x.MomoTransId).HasColumnName("momo_trans_id").HasMaxLength(100);
        builder.Property(x => x.MomoResultCode).HasColumnName("momo_result_code");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
