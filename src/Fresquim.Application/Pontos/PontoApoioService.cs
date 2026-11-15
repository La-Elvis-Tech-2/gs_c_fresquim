using Fresquim.Application.Abstractions;
using Fresquim.Application.Common;
using Fresquim.Domain.Pontos;

namespace Fresquim.Application.Pontos;

public interface IPontoApoioService
{
    Task<PaginaResultado<PontoResponse>> ListarAsync(PontoFiltro filtro, CancellationToken ct = default);
    Task<PontoResponse> ObterAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PontoProximoResponse>> ListarProximosAsync(double latitude, double longitude, double raioKm, TipoPonto? tipo, CancellationToken ct = default);
    Task<PontoResponse> CriarAsync(SalvarPontoRequest request, CancellationToken ct = default);
    Task<PontoResponse> AtualizarAsync(Guid id, SalvarPontoRequest request, CancellationToken ct = default);
    Task<PontoResponse> AlterarStatusAsync(Guid id, StatusPonto status, CancellationToken ct = default);
    Task RemoverAsync(Guid id, CancellationToken ct = default);
}

public class PontoApoioService(IPontoApoioRepository pontos, IUnitOfWork unitOfWork) : IPontoApoioService
{
    public async Task<PaginaResultado<PontoResponse>> ListarAsync(PontoFiltro filtro, CancellationToken ct = default)
    {
        var pagina = await pontos.ListarAsync(filtro, ct);
        return pagina.Mapear(p => p.ParaResponse());
    }

    public async Task<PontoResponse> ObterAsync(Guid id, CancellationToken ct = default) =>
        (await ObterEntidadeAsync(id, ct)).ParaResponse();

    public async Task<IReadOnlyList<PontoProximoResponse>> ListarProximosAsync(
        double latitude, double longitude, double raioKm, TipoPonto? tipo, CancellationToken ct = default)
    {
        var origem = new Coordenada(latitude, longitude);
        var candidatos = await pontos.ListarNaRegiaoAsync(origem, raioKm, ct);

        // LINQ em memória: distância exata, filtro por raio/tipo e ordenação do mais perto para o mais longe.
        return candidatos
            .Where(p => p.EstaDisponivel && (tipo is null || p.Tipo == tipo))
            .Select(p => new { Ponto = p, Distancia = p.DistanciaKm(origem) })
            .Where(x => x.Distancia <= raioKm)
            .OrderBy(x => x.Distancia)
            .Select(x => new PontoProximoResponse(x.Ponto.ParaResponse(), Math.Round(x.Distancia, 2)))
            .ToList();
    }

    public async Task<PontoResponse> CriarAsync(SalvarPontoRequest request, CancellationToken ct = default)
    {
        var ponto = PontoMapper.CriarEntidade(request);
        await pontos.AdicionarAsync(ponto, ct);
        await unitOfWork.SalvarAsync(ct);
        return ponto.ParaResponse();
    }

    public async Task<PontoResponse> AtualizarAsync(Guid id, SalvarPontoRequest request, CancellationToken ct = default)
    {
        var ponto = await ObterEntidadeAsync(id, ct);
        PontoMapper.AplicarEdicao(ponto, request);
        await unitOfWork.SalvarAsync(ct);
        return ponto.ParaResponse();
    }

    public async Task<PontoResponse> AlterarStatusAsync(Guid id, StatusPonto status, CancellationToken ct = default)
    {
        var ponto = await ObterEntidadeAsync(id, ct);

        switch (status)
        {
            case StatusPonto.Funcionando: ponto.MarcarComoFuncionando(); break;
            case StatusPonto.ComProblema: ponto.ReportarProblema(); break;
            case StatusPonto.Desativado: ponto.Desativar(); break;
        }

        await unitOfWork.SalvarAsync(ct);
        return ponto.ParaResponse();
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        var ponto = await ObterEntidadeAsync(id, ct);
        pontos.Remover(ponto);
        await unitOfWork.SalvarAsync(ct);
    }

    private async Task<PontoApoio> ObterEntidadeAsync(Guid id, CancellationToken ct) =>
        await pontos.ObterPorIdAsync(id, ct) ?? throw new RecursoNaoEncontradoException("Ponto de apoio", id);
}
