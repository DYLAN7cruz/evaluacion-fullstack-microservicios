using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ProductsService.Domain.Ports;
using ProductsService.Infrastructure.Persistence;
using ProductsService.Infrastructure.Repositories;

namespace ProductsService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Configuración de base de datos
        var host = Environment.GetEnvironmentVariable("DB_HOST") 
            ?? configuration["DatabaseSettings:Host"] 
            ?? "localhost";

        var portString = Environment.GetEnvironmentVariable("DB_PORT") 
            ?? configuration["DatabaseSettings:Port"] 
            ?? "5432";
        var port = int.TryParse(portString, out var p) ? p : 5432;

        var database = Environment.GetEnvironmentVariable("DB_NAME") 
            ?? configuration["DatabaseSettings:Database"] 
            ?? "inventory_db";

        var username = Environment.GetEnvironmentVariable("DB_USER") 
            ?? configuration["DatabaseSettings:Username"] 
            ?? "postgres";

        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") 
            ?? configuration["DatabaseSettings:Password"] 
            ?? "2004";

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = database,
            Username = username,
            Password = password
        };

        services.AddDbContext<ProductsDbContext>(options =>
            options.UseNpgsql(connectionStringBuilder.ConnectionString));

        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
