using System.Globalization;
using archive.Models;
using Microsoft.Data.Sqlite;

namespace archive.Data
{
	public class BookRepository
	{
		private readonly string _connectionString;

		public BookRepository(string dbPath)
		{
			_connectionString = $"Data Source={dbPath}";
			EnsureDatabase();
		}

		private void EnsureDatabase()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			var command = connection.CreateCommand();
			command.CommandText = @"
CREATE TABLE IF NOT EXISTS Books (
	Id INTEGER PRIMARY KEY AUTOINCREMENT,
	Title TEXT NOT NULL,
	Author TEXT,
	Category TEXT,
	Publisher TEXT,
	Year INTEGER,
	Language TEXT,
	Notes TEXT,
	CoverPath TEXT,
	FilePath TEXT,
	CreatedAt TEXT
);";
			command.ExecuteNonQuery();
		}

		public List<Book> GetAll(string? search = null, string? category = null)
		{
			var books = new List<Book>();
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			var command = connection.CreateCommand();
			command.CommandText = "SELECT Id, Title, Author, Category, Publisher, Year, Language, Notes, CoverPath, FilePath, CreatedAt FROM Books WHERE 1=1";

			if (!string.IsNullOrWhiteSpace(search))
			{
				command.CommandText += " AND (Title LIKE @s OR Author LIKE @s OR Notes LIKE @s)";
				command.Parameters.AddWithValue("@s", $"%{search}%");
			}

			if (!string.IsNullOrWhiteSpace(category))
			{
				command.CommandText += " AND Category = @c";
				command.Parameters.AddWithValue("@c", category);
			}

			command.CommandText += " ORDER BY Id DESC";

			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				books.Add(ReadBook(reader));
			}

			return books;
		}

		public Book? GetById(int id)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			var command = connection.CreateCommand();
			command.CommandText = "SELECT Id, Title, Author, Category, Publisher, Year, Language, Notes, CoverPath, FilePath, CreatedAt FROM Books WHERE Id = @id";
			command.Parameters.AddWithValue("@id", id);
			using var reader = command.ExecuteReader();
			return reader.Read() ? ReadBook(reader) : null;
		}

		public int Add(Book book)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			var command = connection.CreateCommand();
			command.CommandText = @"
INSERT INTO Books (Title, Author, Category, Publisher, Year, Language, Notes, CoverPath, FilePath, CreatedAt)
VALUES (@Title, @Author, @Category, @Publisher, @Year, @Language, @Notes, @CoverPath, @FilePath, @CreatedAt);
SELECT last_insert_rowid();";
			BindParameters(command, book);
			book.CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
			command.Parameters["@CreatedAt"].Value = book.CreatedAt;
			var id = (long)command.ExecuteScalar()!;
			return (int)id;
		}

		public bool Update(Book book)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			var command = connection.CreateCommand();
			command.CommandText = @"
UPDATE Books SET Title = @Title, Author = @Author, Category = @Category, Publisher = @Publisher,
Year = @Year, Language = @Language, Notes = @Notes, CoverPath = @CoverPath, FilePath = @FilePath
WHERE Id = @Id";
			BindParameters(command, book);
			command.Parameters.AddWithValue("@Id", book.Id);
			return command.ExecuteNonQuery() > 0;
		}

		public bool Delete(int id)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			var command = connection.CreateCommand();
			command.CommandText = "DELETE FROM Books WHERE Id = @id";
			command.Parameters.AddWithValue("@id", id);
			return command.ExecuteNonQuery() > 0;
		}

		public List<string> GetCategories()
		{
			var categories = new List<string>();
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			var command = connection.CreateCommand();
			command.CommandText = "SELECT DISTINCT Category FROM Books WHERE Category IS NOT NULL AND Category <> '' ORDER BY Category";
			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				categories.Add(reader.GetString(0));
			}
			return categories;
		}

		private static void BindParameters(SqliteCommand command, Book book)
		{
			command.Parameters.AddWithValue("@Title", book.Title);
			command.Parameters.AddWithValue("@Author", book.Author ?? string.Empty);
			command.Parameters.AddWithValue("@Category", book.Category ?? string.Empty);
			command.Parameters.AddWithValue("@Publisher", book.Publisher ?? string.Empty);
			command.Parameters.AddWithValue("@Year", (object?)book.Year ?? DBNull.Value);
			command.Parameters.AddWithValue("@Language", book.Language ?? string.Empty);
			command.Parameters.AddWithValue("@Notes", book.Notes ?? string.Empty);
			command.Parameters.AddWithValue("@CoverPath", (object?)book.CoverPath ?? DBNull.Value);
			command.Parameters.AddWithValue("@FilePath", (object?)book.FilePath ?? DBNull.Value);
			command.Parameters.AddWithValue("@CreatedAt", book.CreatedAt ?? string.Empty);
		}

		private static Book ReadBook(SqliteDataReader reader)
		{
			return new Book
			{
				Id = reader.GetInt32(0),
				Title = reader.GetString(1),
				Author = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
				Category = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
				Publisher = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
				Year = reader.IsDBNull(5) ? null : reader.GetInt32(5),
				Language = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
				Notes = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
				CoverPath = reader.IsDBNull(8) ? null : reader.GetString(8),
				FilePath = reader.IsDBNull(9) ? null : reader.GetString(9),
				CreatedAt = reader.IsDBNull(10) ? string.Empty : reader.GetString(10)
			};
		}
	}
}
