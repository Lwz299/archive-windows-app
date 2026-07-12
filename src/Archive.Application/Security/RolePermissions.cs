using Archive.Contracts.Responses;
using Archive.Domain.Enums;

namespace Archive.Application.Security
{
    public static class RolePermissions
    {
        public static PermissionResponse For(UserRole role) => role switch
        {
            UserRole.SuperAdmin => new PermissionResponse
            {
                CanViewBooks = true,
                CanManageBooks = true,
                CanManageUsers = true,
                CanCreateUsers = true
            },
            UserRole.Admin => new PermissionResponse
            {
                CanViewBooks = true,
                CanManageBooks = true,
                CanManageUsers = true,
                CanCreateUsers = false
            },
            _ => new PermissionResponse
            {
                CanViewBooks = true,
                CanManageBooks = false,
                CanManageUsers = false,
                CanCreateUsers = false
            }
        };

        public static PermissionResponse For(string? roleName)
        {
            if (Enum.TryParse<UserRole>(roleName, true, out var role))
            {
                return For(role);
            }

            return For(UserRole.User);
        }
    }
}
