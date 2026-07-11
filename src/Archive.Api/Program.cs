using Archive.Application;
using Archive.Infrastructure;
using Archive.Application.Interfaces;
using Archive.Domain.Entities;
using Archive.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSection = builder.Configuration.GetSection("Jwt");
var secret = jwtSection.GetValue<string>("Secret") ?? throw new InvalidOperationException("JWT secret is required.");
var issuer = jwtSection.GetValue<string>("Issuer") ?? "ArchiveApi";
var audience = jwtSection.GetValue<string>("Audience") ?? "ArchiveApiUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Render and most hosts terminate TLS at the proxy; the app listens on HTTP.
if (!app.Environment.IsDevelopment() && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RENDER")))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Create schema once at startup, then seed default users if empty
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Archive.Infrastructure.Persistence.ArchiveDbContext>();
    Archive.Infrastructure.Repositories.BookRepository.EnsureSchema(db);

    var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var hasAny = userRepo.HasAnyUserAsync().GetAwaiter().GetResult();
    if (!hasAny)
    {
        var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        var defaults = new[] {
            (Username: "admin", Password: "AdminPass123!", Role: UserRole.SuperAdmin),
            (Username: "librarian", Password: "LibPass123!", Role: UserRole.Admin),
            (Username: "user", Password: "UserPass123!", Role: UserRole.User)
        };

        foreach (var d in defaults)
        {
            var salt = passwordHasher.GenerateSalt();
            var hash = passwordHasher.Hash(d.Password, salt);
            var u = new User
            {
                Username = d.Username,
                PasswordHash = hash,
                Salt = salt,
                Role = d.Role,
                CreatedAt = now
            };
            userRepo.AddAsync(u).GetAwaiter().GetResult();
        }
    }
}


app.Run();
