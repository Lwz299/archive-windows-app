using Archive.Application.Interfaces;
using Archive.Infrastructure.Persistence;
using Archive.Infrastructure.Repositories;
using Archive.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Archive.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=archive.db";

            services.AddDbContext<ArchiveDbContext>(options =>
            {
                if (IsPostgreSql(connectionString))
                {
                    options.UseNpgsql(connectionString);
                }
                else
                {
                    options.UseSqlite(connectionString);
                }
            });

            services.AddScoped(typeof(Archive.Application.Interfaces.IUserRepository), typeof(UserRepository));
            services.AddScoped(typeof(Archive.Application.Interfaces.IBookRepository), typeof(BookRepository));
            services.AddSingleton<Archive.Application.Interfaces.IPasswordHasher, PasswordHasher>();
            services.AddSingleton<Archive.Application.Interfaces.IJwtTokenGenerator>(provider =>
            {
                var jwtSection = configuration.GetSection("Jwt");
                var secret = jwtSection.GetValue<string>("Secret") ?? throw new InvalidOperationException("JWT secret is required");
                var issuer = jwtSection.GetValue<string>("Issuer") ?? "ArchiveApi";
                var audience = jwtSection.GetValue<string>("Audience") ?? "ArchiveApiUsers";
                return new JwtTokenGenerator(secret, issuer, audience);
            });

            return services;
        }

        private static bool IsPostgreSql(string connectionString)
        {
            return connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase)
                || connectionString.Contains("Username=", StringComparison.OrdinalIgnoreCase)
                || connectionString.Contains("User ID=", StringComparison.OrdinalIgnoreCase);
        }
    }
}
