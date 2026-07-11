using Archive.Domain.Entities;

namespace Archive.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
