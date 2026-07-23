using Microsoft.Extensions.DependencyInjection;
using MikroProje.Application.Interfaces;
using MikroProje.Infrastructure.Services;

namespace MikroProje.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
