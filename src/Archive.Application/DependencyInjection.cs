using Archive.Application.Interfaces;
using Archive.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Archive.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICategoryService, CategoryService>();

            return services;
        }
    }
}
