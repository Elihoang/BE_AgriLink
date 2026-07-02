using AgriLink_DH.Domain.Common;

using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

public class User : BaseEntity
{

                public string Username { get; set; } = string.Empty;

                public string Email { get; set; } = string.Empty;

            public string PasswordHash { get; set; } = string.Empty;

            public string? FullName { get; set; }

            public string? PhoneNumber { get; set; }

            public string? Address { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        public bool IsActive { get; set; } = true;

            public string? ImageUrl { get; set; } // URL hình ảnh người dùng/avatar

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
}
