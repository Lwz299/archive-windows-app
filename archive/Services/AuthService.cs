using System.Globalization;
using System.Security.Cryptography;
using archive.Data;
using archive.Models;

namespace archive.Services
{
	public class AuthService
	{
		private const int SaltSize = 16;
		private const int HashSize = 32;
		private const int Iterations = 100_000;

		private readonly UserRepository _repository;

		public AuthService(string appDataFolder)
		{
			var dbPath = Path.Combine(appDataFolder, "archive.db");
			_repository = new UserRepository(dbPath);
		}

		public bool HasAnyUser() => _repository.HasAnyUser();

		public (bool Success, string? Error) Register(string username, string password)
		{
			username = username.Trim();

			if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
			{
				return (false, "اسم المستخدم يجب أن يتكون من 3 أحرف على الأقل");
			}

			if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
			{
				return (false, "كلمة المرور يجب أن تتكون من 4 أحرف على الأقل");
			}

			if (_repository.GetByUsername(username) != null)
			{
				return (false, "اسم المستخدم موجود مسبقاً");
			}

			var salt = RandomNumberGenerator.GetBytes(SaltSize);
			var hash = HashPassword(password, salt);

			_repository.Add(new User
			{
				Username = username,
				PasswordHash = Convert.ToBase64String(hash),
				Salt = Convert.ToBase64String(salt),
				CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
			});

			return (true, null);
		}

		public (bool Success, string? Error) Login(string username, string password)
		{
			var user = _repository.GetByUsername(username.Trim());
			if (user == null)
			{
				return (false, "اسم المستخدم أو كلمة المرور غير صحيحة");
			}

			var salt = Convert.FromBase64String(user.Salt);
			var expectedHash = Convert.FromBase64String(user.PasswordHash);
			var actualHash = HashPassword(password, salt);

			if (!CryptographicOperations.FixedTimeEquals(expectedHash, actualHash))
			{
				return (false, "اسم المستخدم أو كلمة المرور غير صحيحة");
			}

			return (true, null);
		}

		private static byte[] HashPassword(string password, byte[] salt)
		{
			return Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
		}
	}
}
