using Fresquim.Application.Abstractions;
using Fresquim.Application.Common;
using Fresquim.Domain.Avisos;
using Fresquim.Domain.Common;

namespace Fresquim.Application.Avisos;

public interface IAvisoService
{
    Task<PaginaResultado<AvisoResponse>> ListarAsync(AvisoFiltro filtro, CancellationToken ct = default);
    Task<AvisoResponse> ObterAsync(Guid id, CancellationToken ct = default);
    Task<AvisoResponse> ReportarProblemaAsync(ReportarProblemaRequest request, CancellationToken ct = default);
    Task<AvisoResponse> SugerirPontoAsync(SugerirPontoRequest request, CancellationToken ct = default);
    Task<AvisoResponse> IniciarAnaliseAsync(Guid id, CancellationToken ct = default);
    Task<AvisoResponse> ResolverAsync(Guid id, string resposta, CancellationToken ct = default);
    Task<AvisoResponse> DescartarAsync(Guid id, string motivo, CancellationToken ct = default);
    Task RemoverAsync(Guid id, CancellationToken ct = default);
}

public class AvisoService(IAvisoRepository avisos, IPontoApoioRepository pontos, IUnitOfWork unitOfWork) : IAvisoService
{
    public async Task<PaginaResultado<AvisoResponse>> ListarAsync(AvisoFiltro filtro, CancellationToken ct = default) =>
        (await avisos.ListarAsync(filtro, ct)).Mapear(ParaResponse);

    public async Task<AvisoResponse> ObterAsync(Guid id, CancellationToken ct = default) =>
        ParaResponse(await ObterEntidadeAsync(id, ct));

    public async Task<AvisoResponse> ReportarProblemaAsync(ReportarProblemaRequest request, CancellationToken ct = default)
    {
        var ponto = await pontos.ObterPorIdAsync(request.PontoApoioId, ct)
                    ?? throw new RecursoNaoEncontradoException("Ponto de apoio", request.PontoApoioId);

        // Evita avisos duplicados para o mesmo ponto enquanto o anterior não for finalizado.
        if (await avisos.PontoTemAvisoAbertoAsync(ponto.Id, ct))
            throw new DomainException("Já existe um aviso em aberto para este ponto. Obrigado por avisar!");

        var aviso = Aviso.ReportarProblema(ponto, request.Descricao);
        await avisos.AdicionarAsync(aviso, ct);
        await unitOfWork.SalvarAsync(ct);
        return ParaResponse(aviso);
    }

    public async Task<AvisoResponse> SugerirPontoAsync(SugerirPontoRequest request, CancellationToken ct = default)
    {
        var aviso = Aviso.SugerirPonto(request.NomeLocal, request.Descricao);
        await avisos.AdicionarAsync(aviso, ct);
        await unitOfWork.SalvarAsync(ct);
        return ParaResponse(aviso);
    }

    public Task<AvisoResponse> IniciarAnaliseAsync(Guid id, CancellationToken ct = default) =>
        AlterarAsync(id, a => a.IniciarAnalise(), ct);

    public Task<AvisoResponse> ResolverAsync(Guid id, string resposta, CancellationToken ct = default) =>
        AlterarAsync(id, a => a.Resolver(resposta), ct);

    public Task<AvisoResponse> DescartarAsync(Guid id, string motivo, CancellationToken ct = default) =>
        AlterarAsync(id, a => a.Descartar(motivo), ct);

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        var aviso = await ObterEntidadeAsync(id, ct);
        avisos.Remover(aviso);
        await unitOfWork.SalvarAsync(ct);
    }

    private async Task<AvisoResponse> AlterarAsync(Guid id, Action<Aviso> acao, CancellationToken ct)
    {
        var aviso = await ObterEntidadeAsync(id, ct);
        acao(aviso);
        await unitOfWork.SalvarAsync(ct);
        return ParaResponse(aviso);
    }

    private async Task<Aviso> ObterEntidadeAsync(Guid id, CancellationToken ct) =>
        await avisos.ObterPorIdAsync(id, ct) ?? throw new RecursoNaoEncontradoException("Aviso", id);

    private static AvisoResponse ParaResponse(Aviso a) => new(
        a.Id, a.Tipo, a.Status, a.Descricao, a.PontoApoioId, a.PontoApoio?.Nome,
        a.NomeLocalSugerido, a.RespostaPrefeitura, a.CriadoEm, a.ResolvidoEm);
}
