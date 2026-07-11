using archive.Models;
using Microsoft.Data.Sqlite;

namespace archive.Data
{
	public class UserRepository
	{
		private readonly string _connectionString;

		public UserRepository(string dbPath)
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
CREATE TABLE IF NOT EXISTS Users (
	Id INTEGER PRIMARY KEY AUTOINCREMENT,
	Username TEXT NOT NULL UNIQUE,
	PasswordHash TEXT NOT NULL,
	Salt TEXT NOT NULL,
	CreatedAt TEXT
);";
			command.ExecuteNonQuery();
		}

		public bool HasAnyUser()
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			var command = connection.CreateCommand();
			command.CommandText = "SELECT COUNT(*) FROM Users";
			var count = (long)command.ExecuteScalar()!;
			return count > 0;
		}

		public User? GetByUsername(string username)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			var command = connection.CreateCommand();
			command.CommandText = "SELECT Id, Username, PasswordHash, Salt, CreatedAt FROM Users WHERE Username = @u COLLATE NOCASE";
			command.Parameters.AddWithValue("@u", username);
			using var reader = command.ExecuteReader();
			if (!reader.Read())
			{
				return null;
			}

			return new User
			{
				Id = reader.GetInt32(0),
				Username = reader.GetString(1),
				PasswordHash = reader.GetString(2),
				Salt = reader.GetString(3),
				CreatedAt = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
			};
		}

		public int Add(User user)
		{
			using var connection = new SqliteConnection(_connectionString);
			connection.Open();
			var command = connection.CreateCommand();
			command.CommandText = @"
INSERT INTO Users (Username, PasswordHash, Salt, CreatedAt)
VALUES (@Username, @PasswordHash, @Salt, @CreatedAt);
SELECT last_insert_rowid();";
			command.Parameters.AddWithValue("@Username", user.Username);
			command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
			command.Parameters.AddWithValue("@Salt", user.Salt);
			command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
			var id = (long)command.ExecuteScalar()!;
			return (int)id;
		}
	}
}
