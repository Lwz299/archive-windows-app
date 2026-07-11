namespace Archive.Application.Interfaces
{
    public interface IPasswordHasher
    {
        string GenerateSalt();
        string Hash(string password, string salt);
        bool Verify(string password, string hashedPassword, string salt);
    }
}
