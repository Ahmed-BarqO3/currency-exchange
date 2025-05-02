using Currencey.Api.Database;

namespace Currencey.Api;

public static class ServiceExtensions
{
    public static IServiceCollection AddDataBase(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(new NpgsDbConnectionFactory(connectionString));
        services.AddSingleton<DBInitializer>();
        
        return services;
    }
    
}
