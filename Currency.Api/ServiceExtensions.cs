using Currencey.Api.Database;
using Currencey.Api.Repository;

namespace Currencey.Api;

public static class ServiceExtensions
{
    public static IServiceCollection AddDataBase(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(new NpgsDbConnectionFactory(connectionString));
        services.AddSingleton<DBInitializer>();
        
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
    
}
