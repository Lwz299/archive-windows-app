namespace archive.Models
{
	public class User
	{
		public int Id { get; set; }
		public string Username { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public string Salt { get; set; } = string.Empty;
		public string CreatedAt { get; set; } = string.Empty;
	}
}
