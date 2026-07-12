using System.Security.Claims;
using Archive.Application.Interfaces;
using Archive.Application.Security;
using Archive.Contracts.Requests;
using Archive.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>هل يوجد مستخدمون في النظام؟</summary>
        [HttpGet("setup")]
        public async Task<ActionResult<bool>> CheckSetup()
        {
            return Ok(await _authService.HasAnyUserAsync());
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            try
            {
                return Ok(await _authService.LoginAsync(request));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        /// <summary>
        /// تسجيل عام: بعد وجود مستخدمين يُنشئ دائماً دور User فقط.
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>معلومات الجلسة الحالية + الصلاحيات</summary>
        [Authorize]
        [HttpGet("me")]
        public ActionResult<object> Me()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
            return Ok(new
            {
                username,
                role,
                permissions = RolePermissions.For(role)
            });
        }
    }
}
