namespace Archive.Contracts.Responses
{
    public class PermissionResponse
    {
        public bool CanViewBooks { get; set; }
        public bool CanManageBooks { get; set; }
        public bool CanManageUsers { get; set; }
        public bool CanCreateUsers { get; set; }
    }
}
