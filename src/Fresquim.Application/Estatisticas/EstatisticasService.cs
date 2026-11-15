using Fresquim.Application.Abstractions;
using Fresquim.Domain.Avisos;
using Fresquim.Domain.Pontos;

namespace Fresquim.Application.Estatisticas;

public sealed record ContagemPorChave(string Chave, int Total);

public sealed record BairroResumo(string Bairro, int TotalPontos, int PontosComProblema, int AvisosAbertos);

/// <summary>Indicadores de impacto da CitizenTech (usados na Ficha da GS).</summary>
public sealed record EstatisticasResponse(
    int TotalPontos,
    int PontosDisponiveis,
    double PercentualDisponivel,
    int TotalAvisos,
    int AvisosResolvidos,
    double? TempoMedioResolucaoHoras,
    IReadOnlyList<ContagemPorChave> PontosPorTipo,
    IReadOnlyList<ContagemPorChave> AvisosPorStatus,
    IReadOnlyList<BairroResumo> Bairros);

public interface IEstatisticasService
{
    Task<EstatisticasResponse> ObterAsync(CancellationToken ct = default);
}

public class EstatisticasService(IPontoApoioRepository pontosRepo, IAvisoRepository avisosRepo) : IEstatisticasService
{
    public async Task<EstatisticasResponse> ObterAsync(CancellationToken ct = default)
    {
        var pontos = await pontosRepo.ListarTodosAsync(ct);
        var avisos = await avisosRepo.ListarTodosAsync(ct);

        var disponiveis = pontos.Count(p => p.EstaDisponivel);
        var resolvidos = avisos.Where(a => a.Status == StatusAviso.Resolvido && a.ResolvidoEm.HasValue).ToList();

        var pontosPorTipo = pontos
            .GroupBy(p => p.Tipo)
            .OrderBy(g => g.Key)
            .Select(g => new ContagemPorChave(g.Key.ToString(), g.Count()))
            .ToList();

        var avisosPorStatus = avisos
            .GroupBy(a => a.Status)
            .OrderBy(g => g.Key)
            .Select(g => new ContagemPorChave(g.Key.ToString(), g.Count()))
            .ToList();

        // Junta pontos e avisos abertos por bairro (GroupBy + GroupJoin).
        var bairros = pontos
            .GroupBy(p => p.Endereco.Bairro)
            .GroupJoin(
                avisos.Where(a => !a.EstaFinalizado && a.PontoApoio is not null),
                g => g.Key,
                a => a.PontoApoio!.Endereco.Bairro,
                (g, avisosDoBairro) => new BairroResumo(
                    g.Key,
                    g.Count(),
                    g.Count(p => p.Status == StatusPonto.ComProblema),
                    avisosDoBairro.Count()))
            .OrderByDescending(b => b.AvisosAbertos)
            .ThenBy(b => b.Bairro)
            .ToList();

        return new EstatisticasResponse(
            pontos.Count,
            disponiveis,
            pontos.Count == 0 ? 0 : Math.Round(disponiveis * 100.0 / pontos.Count, 1),
            avisos.Count,
            resolvidos.Count,
            resolvidos.Count == 0 ? null : Math.Round(resolvidos.Average(a => (a.ResolvidoEm!.Value - a.CriadoEm).TotalHours), 1),
            pontosPorTipo,
            avisosPorStatus,
            bairros);
    }
}
