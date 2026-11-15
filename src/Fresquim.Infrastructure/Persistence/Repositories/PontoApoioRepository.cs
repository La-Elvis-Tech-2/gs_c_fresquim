using Fresquim.Application.Abstractions;
using Fresquim.Application.Common;
using Fresquim.Application.Pontos;
using Fresquim.Domain.Pontos;
using Fresquim.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Fresquim.Infrastructure.Persistence.Repositories;

public class PontoApoioRepository(FresquimDbContext db) : IPontoApoioRepository
{
    private const double KmPorGrauLatitude = 111.32;

    public Task<PontoApoio?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Pontos.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PaginaResultado<PontoApoio>> ListarAsync(PontoFiltro filtro, CancellationToken ct = default)
    {
        IQueryable<PontoApoio> query = db.Pontos.AsNoTracking();

        // Cada filtro só entra na consulta SQL se foi informado.
        if (filtro.Tipo is { } tipo)
            query = query.Where(p => EF.Property<TipoPonto>(p, PontoApoioConfiguration.ColunaTipo) == tipo);

        if (filtro.Status is { } status)
            query = query.Where(p => p.Status == status);

        if (!string.IsNullOrWhiteSpace(filtro.Bairro))
        {
            var bairro = filtro.Bairro.Trim().ToLower();
            query = query.Where(p => p.Endereco.Bairro.ToLower() == bairro);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var termo = $"%{filtro.Busca.Trim()}%";
            query = query.Where(p => EF.Functions.Like(p.Nome, termo) || EF.Functions.Like(p.Endereco.Logradouro, termo));
        }

        var total = await query.CountAsync(ct);
        var itens = await query
            .OrderBy(p => p.Nome)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .ToListAsync(ct);

        return new PaginaResultado<PontoApoio>(itens, filtro.Pagina, filtro.TamanhoPagina, total);
    }

    public async Task<IReadOnlyList<PontoApoio>> ListarNaRegiaoAsync(Coordenada centro, double raioKm, CancellationToken ct = default)
    {
        // "Caixa" de latitude/longitude ao redor do centro: o banco filtra o grosso,
        // e o serviço calcula a distância exata (Haversine) só para esses pontos.
        var deltaLat = raioKm / KmPorGrauLatitude;
        var deltaLon = raioKm / (KmPorGrauLatitude * Math.Cos(centro.Latitude * Math.PI / 180));

        return await db.Pontos.AsNoTracking()
            .Where(p => p.Localizacao.Latitude >= centro.Latitude - deltaLat
                        && p.Localizacao.Latitude <= centro.Latitude + deltaLat
                        && p.Localizacao.Longitude >= centro.Longitude - deltaLon
                        && p.Localizacao.Longitude <= centro.Longitude + deltaLon)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PontoApoio>> ListarTodosAsync(CancellationToken ct = default) =>
        await db.Pontos.AsNoTracking().ToListAsync(ct);

    public async Task AdicionarAsync(PontoApoio ponto, CancellationToken ct = default) =>
        await db.Pontos.AddAsync(ponto, ct);

    public void Remover(PontoApoio ponto) => db.Pontos.Remove(ponto);
}
