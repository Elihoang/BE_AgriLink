using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriLink_DH.Infrastructure.Configurations;

public class UserLoginLogConfiguration : IEntityTypeConfiguration<UserLoginLog>
{
    public void Configure(EntityTypeBuilder<UserLoginLog> builder)
    {
        builder.ToTable("user_login_logs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).IsRequired().HasColumnName("user_id");
        builder.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
        builder.Property(x => x.DeviceInfo).HasColumnName("device_info").HasMaxLength(500);
        builder.Property(x => x.LoginTime).HasColumnName("login_time");
        builder.Property(x => x.IsSuccess).HasColumnName("is_success");
        builder.Property(x => x.Metadata).HasColumnName("metadata");
        builder.Property(x => x.ActionType).HasColumnName("action_type");
    }
}
