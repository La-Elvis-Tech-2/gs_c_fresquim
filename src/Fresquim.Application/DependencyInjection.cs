using Fresquim.Application.Avisos;
using Fresquim.Application.Clima;
using Fresquim.Application.Estatisticas;
using Fresquim.Application.Pontos;
using Microsoft.Extensions.DependencyInjection;

namespace Fresquim.Application;

public static class DependencyInjection
{
    /// <summary>Registra os serviços de aplicação (regras de uso) no contêiner de DI.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPontoApoioService, PontoApoioService>();
        services.AddScoped<IAvisoService, AvisoService>();
        services.AddScoped<IClimaService, ClimaService>();
        services.AddScoped<IEstatisticasService, EstatisticasService>();
        return services;
    }
}
