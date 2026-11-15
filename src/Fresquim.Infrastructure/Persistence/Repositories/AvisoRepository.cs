using Fresquim.Application.Abstractions;
using Fresquim.Application.Avisos;
using Fresquim.Application.Common;
using Fresquim.Domain.Avisos;
using Microsoft.EntityFrameworkCore;

namespace Fresquim.Infrastructure.Persistence.Repositories;

public class AvisoRepository(FresquimDbContext db) : IAvisoRepository
{
    public Task<Aviso?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Avisos.Include(a => a.PontoApoio).FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<PaginaResultado<Aviso>> ListarAsync(AvisoFiltro filtro, CancellationToken ct = default)
    {
        var query = db.Avisos.AsNoTracking().Include(a => a.PontoApoio).AsQueryable();

        if (filtro.Tipo is { } tipo) query = query.Where(a => a.Tipo == tipo);
        if (filtro.Status is { } status) query = query.Where(a => a.Status == status);
        if (filtro.PontoApoioId is { } pontoId) query = query.Where(a => a.PontoApoioId == pontoId);

        var total = await query.CountAsync(ct);
        var itens = await query
            .OrderByDescending(a => a.CriadoEm)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .ToListAsync(ct);

        return new PaginaResultado<Aviso>(itens, filtro.Pagina, filtro.TamanhoPagina, total);
    }

    public async Task<IReadOnlyList<Aviso>> ListarTodosAsync(CancellationToken ct = default) =>
        await db.Avisos.AsNoTracking().Include(a => a.PontoApoio).ToListAsync(ct);

    public Task<bool> PontoTemAvisoAbertoAsync(Guid pontoId, CancellationToken ct = default) =>
        db.Avisos.AnyAsync(a => a.PontoApoioId == pontoId
                                && (a.Status == StatusAviso.Aberto || a.Status == StatusAviso.EmAnalise), ct);

    public async Task AdicionarAsync(Aviso aviso, CancellationToken ct = default) =>
        await db.Avisos.AddAsync(aviso, ct);

    public void Remover(Aviso aviso) => db.Avisos.Remove(aviso);
}
