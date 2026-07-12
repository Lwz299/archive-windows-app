using Archive.Contracts.Responses;

namespace Archive.Desktop.Services
{
    public class SessionService
    {
        public string? Username { get; private set; }
        public string? Role { get; private set; }
        public string? Token { get; private set; }
        public PermissionResponse Permissions { get; private set; } = new();

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

        public void SetSession(string username, string role, string token, PermissionResponse? permissions = null)
        {
            Username = username;
            Role = role;
            Token = token;
            Permissions = permissions ?? new PermissionResponse
            {
                CanViewBooks = true,
                CanManageBooks = role is "Admin" or "SuperAdmin",
                CanManageUsers = role is "Admin" or "SuperAdmin",
                CanCreateUsers = role == "SuperAdmin"
            };
        }

        public void ClearSession()
        {
            Username = null;
            Role = null;
            Token = null;
            Permissions = new();
        }
    }
}
