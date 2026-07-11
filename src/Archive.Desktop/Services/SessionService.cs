namespace Archive.Desktop.Services
{
    public class SessionService
    {
        public string? Username { get; private set; }
        public string? Role { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);
        public string? Token { get; private set; }

        public void SetSession(string username, string role, string token)
        {
            Username = username;
            Role = role;
            Token = token;
        }

        public void ClearSession()
        {
            Username = null;
            Role = null;
            Token = null;
        }
    }
}
