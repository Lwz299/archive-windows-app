namespace archive.Models
{
	public class Book
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Author { get; set; } = string.Empty;
		public string Category { get; set; } = string.Empty;
		public string Publisher { get; set; } = string.Empty;
		public int? Year { get; set; }
		public string Language { get; set; } = string.Empty;
		public string Notes { get; set; } = string.Empty;
		public string? CoverPath { get; set; }
		public string? FilePath { get; set; }
		public string CreatedAt { get; set; } = string.Empty;
	}
}
