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
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? configuration["DATABASE_URL"]
                ?? "Data Source=archive.db";

            connectionString = NormalizeConnectionString(connectionString);
            var usePostgres = IsPostgreSql(connectionString);

            services.AddDbContext<ArchiveDbContext>(options =>
            {
                if (usePostgres)
                {
                    options.UseNpgsql(connectionString);
                }
                else
                {
                    options.UseSqlite(connectionString);
                }
            });

            services.AddScoped(typeof(IUserRepository), typeof(UserRepository));
            services.AddScoped(typeof(IBookRepository), typeof(BookRepository));
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<IJwtTokenGenerator>(_ =>
            {
                var jwtSection = configuration.GetSection("Jwt");
                var secret = jwtSection.GetValue<string>("Secret") ?? throw new InvalidOperationException("JWT secret is required");
                var issuer = jwtSection.GetValue<string>("Issuer") ?? "ArchiveApi";
                var audience = jwtSection.GetValue<string>("Audience") ?? "ArchiveApiUsers";
                return new JwtTokenGenerator(secret, issuer, audience);
            });

            return services;
        }

        internal static string NormalizeConnectionString(string connectionString)
        {
            if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
                || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(connectionString);
                var userInfo = uri.UserInfo.Split(':', 2);
                var username = Uri.UnescapeDataString(userInfo[0]);
                var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
                var database = uri.AbsolutePath.Trim('/');
                var port = uri.Port > 0 ? uri.Port : 5432;

                return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Prefer;Trust Server Certificate=true;Pooling=false";
            }

            return connectionString;
        }

        private static bool IsPostgreSql(string connectionString)
        {
            return connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
                || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
                || connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase)
                || connectionString.Contains("Username=", StringComparison.OrdinalIgnoreCase)
                || connectionString.Contains("User ID=", StringComparison.OrdinalIgnoreCase);
        }
    }
}
