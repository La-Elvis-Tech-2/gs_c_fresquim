using Fresquim.Application.Abstractions;
using Fresquim.Infrastructure.Clima;
using Fresquim.Infrastructure.Persistence;
using Fresquim.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fresquim.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registra banco (EF Core + SQLite), repositórios e o cliente da Open-Meteo.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Fresquim") ?? "Data Source=fresquim.db";

        services.AddDbContext<FresquimDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FresquimDbContext>());
        services.AddScoped<IPontoApoioRepository, PontoApoioRepository>();
        services.AddScoped<IAvisoRepository, AvisoRepository>();

        services.AddHttpClient<IClimaProvider, OpenMeteoClimaProvider>(client =>
        {
            client.BaseAddress = new Uri(configuration["OpenMeteo:BaseUrl"] ?? "https://api.open-meteo.com/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
