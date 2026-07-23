using MikroProje.Persistence.Contexts;
using MikroProje.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MikroProje.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MikroProjeDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<MikroProje.Application.Interfaces.ICurrentAccountRepository, CurrentAccountRepository>();
        services.AddScoped<MikroProje.Application.Interfaces.IProductRepository, ProductRepository>();
        services.AddScoped<MikroProje.Application.Interfaces.IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<MikroProje.Application.Interfaces.ISaleRepository, SaleRepository>();
        services.AddScoped<MikroProje.Application.Interfaces.IPaymentRepository, PaymentRepository>();

        return services;
    }
}
