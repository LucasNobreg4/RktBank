using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IBankAccountService, BankAccountService>();

            services.AddMemoryCache();
            services.AddSingleton<IMemoryCacheService, MemoryCacheService>();
            return services;
        }
    }
}
