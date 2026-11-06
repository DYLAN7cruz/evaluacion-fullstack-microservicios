using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TransactionsService.Domain.Ports;
using TransactionsService.Infrastructure.Persistence;
using TransactionsService.Infrastructure.Repositories;

namespace TransactionsService.Infrastructure;

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

        services.AddDbContext<TransactionsDbContext>(options =>
            options.UseNpgsql(connectionStringBuilder.ConnectionString));

        services.AddScoped<ITransactionRepository, TransactionRepository>();

        return services;
    }
}
