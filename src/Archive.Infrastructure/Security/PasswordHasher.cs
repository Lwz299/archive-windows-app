using Archive.Application.Interfaces;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Archive.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int Iterations = 100_000;
        private const int KeySize = 32;

        public string GenerateSalt()
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            return Convert.ToBase64String(salt);
        }

        public string Hash(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            var hash = KeyDerivation.Pbkdf2(password, saltBytes, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);
            return Convert.ToBase64String(hash);
        }

        public bool Verify(string password, string hashedPassword, string salt)
        {
            var computedHash = Hash(password, salt);
            return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(hashedPassword), Convert.FromBase64String(computedHash));
        }
    }
}
