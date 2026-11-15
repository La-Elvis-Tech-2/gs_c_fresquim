using Fresquim.Application.Abstractions;
using Fresquim.Application.Pontos;
using Fresquim.Domain.Clima;
using Fresquim.Domain.Pontos;

namespace Fresquim.Application.Clima;

public sealed record RiscoCalorResponse(
    double Temperatura,
    double SensacaoTermica,
    double MaximaDoDia,
    double IndiceUv,
    NivelRisco Nivel,
    string Rotulo,
    string Orientacao,
    DateTime MedidoEm,
    string Fonte,
    IReadOnlyList<PontoProximoResponse> PontosRecomendados);

public interface IClimaService
{
    Task<RiscoCalorResponse> AvaliarRiscoAsync(double latitude, double longitude, double raioKm, CancellationToken ct = default);
}

public class ClimaService(IClimaProvider clima, IPontoApoioRepository pontos) : IClimaService
{
    private const int MaximoRecomendados = 5;

    public async Task<RiscoCalorResponse> AvaliarRiscoAsync(double latitude, double longitude, double raioKm, CancellationToken ct = default)
    {
        var local = new Coordenada(latitude, longitude);

        // As duas consultas são independentes: rodam em paralelo (API externa + banco).
        var condicaoTask = clima.ObterCondicaoAtualAsync(local, ct);
        var pontosTask = pontos.ListarNaRegiaoAsync(local, raioKm, ct);
        await Task.WhenAll(condicaoTask, pontosTask);

        var condicao = await condicaoTask;
        var risco = RiscoCalor.Classificar(condicao);

        // Cada tipo de ponto decide se é recomendado no nível atual (polimorfismo).
        var recomendados = (await pontosTask)
            .Where(p => p.RecomendadoPara(risco.Nivel))
            .Select(p => new { Ponto = p, Distancia = p.DistanciaKm(local) })
            .Where(x => x.Distancia <= raioKm)
            .OrderBy(x => x.Distancia)
            .Take(MaximoRecomendados)
            .Select(x => new PontoProximoResponse(x.Ponto.ParaResponse(), Math.Round(x.Distancia, 2)))
            .ToList();

        return new RiscoCalorResponse(
            condicao.Temperatura, condicao.SensacaoTermica, condicao.MaximaDoDia, condicao.IndiceUv,
            risco.Nivel, risco.Rotulo, risco.Orientacao, condicao.MedidoEm, "Open-Meteo", recomendados);
    }
}
