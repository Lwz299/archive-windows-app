using Archive.Application.Interfaces;
using Archive.Contracts.Requests;
using Archive.Contracts.Responses;
using Archive.Domain.Enums;
using Archive.Domain.Entities;

namespace Archive.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public Task<bool> HasAnyUserAsync()
        {
            return _userRepository.HasAnyUserAsync();
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash, user.Salt))
            {
                throw new UnauthorizedAccessException("اسم المستخدم أو كلمة المرور غير صحيحة");
            }

            return new LoginResponse
            {
                Username = user.Username,
                Role = user.Role.ToString(),
                Token = _jwtTokenGenerator.GenerateToken(user)
            };
        }

        public async Task<UserResponse> RegisterAsync(RegisterRequest request)
        {
            var exists = await _userRepository.HasAnyUserAsync();
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException("اسم المستخدم موجود مسبقاً");
            }

            var role = exists
                ? (Enum.TryParse<UserRole>(request.Role, true, out var parsed) ? parsed : UserRole.User)
                : UserRole.SuperAdmin;

            var salt = _passwordHasher.GenerateSalt();
            var hash = _passwordHasher.Hash(request.Password, salt);
            var user = new User
            {
                Username = request.Username.Trim(),
                PasswordHash = hash,
                Salt = salt,
                Role = role,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")
            };

            user.Id = await _userRepository.AddAsync(user);

            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }
    }
}
