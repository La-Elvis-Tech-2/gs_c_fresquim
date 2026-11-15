using Fresquim.Application.Common;
using Fresquim.Application.Pontos;
using Fresquim.Application.Avisos;
using Fresquim.Domain.Avisos;
using Fresquim.Domain.Pontos;

namespace Fresquim.Application.Abstractions;

public interface IPontoApoioRepository
{
    Task<PontoApoio?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lista paginada com filtros por tipo, status, bairro e busca por texto.</summary>
    Task<PaginaResultado<PontoApoio>> ListarAsync(PontoFiltro filtro, CancellationToken ct = default);

    /// <summary>Pontos dentro de uma "caixa" ao redor da coordenada (pré-filtro para o cálculo de distância).</summary>
    Task<IReadOnlyList<PontoApoio>> ListarNaRegiaoAsync(Coordenada centro, double raioKm, CancellationToken ct = default);

    Task<IReadOnlyList<PontoApoio>> ListarTodosAsync(CancellationToken ct = default);

    Task AdicionarAsync(PontoApoio ponto, CancellationToken ct = default);

    void Remover(PontoApoio ponto);
}

public interface IAvisoRepository
{
    Task<Aviso?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginaResultado<Aviso>> ListarAsync(AvisoFiltro filtro, CancellationToken ct = default);

    Task<IReadOnlyList<Aviso>> ListarTodosAsync(CancellationToken ct = default);

    Task<bool> PontoTemAvisoAbertoAsync(Guid pontoId, CancellationToken ct = default);

    Task AdicionarAsync(Aviso aviso, CancellationToken ct = default);

    void Remover(Aviso aviso);
}

/// <summary>Confirma as alterações de uma operação em uma única transação.</summary>
public interface IUnitOfWork
{
    Task<int> SalvarAsync(CancellationToken ct = default);
}
