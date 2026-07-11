using Archive.Domain.Enums;

namespace Archive.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public string CreatedAt { get; set; } = string.Empty;
    }
}
