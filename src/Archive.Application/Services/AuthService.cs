using Archive.Application.Interfaces;
using Archive.Application.Security;
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
                Token = _jwtTokenGenerator.GenerateToken(user),
                Permissions = RolePermissions.For(user.Role)
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

            // First user ever = SuperAdmin. After seed/users exist, public register is always User only.
            var role = exists ? UserRole.User : UserRole.SuperAdmin;

            return await CreateUserInternalAsync(request.Username, request.Password, role);
        }

        public async Task<UserResponse> CreateUserAsAdminAsync(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException("اسم المستخدم موجود مسبقاً");
            }

            if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            {
                role = UserRole.User;
            }

            return await CreateUserInternalAsync(request.Username, request.Password, role);
        }

        private async Task<UserResponse> CreateUserInternalAsync(string username, string password, UserRole role)
        {
            var salt = _passwordHasher.GenerateSalt();
            var hash = _passwordHasher.Hash(password, salt);
            var user = new User
            {
                Username = username.Trim(),
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
