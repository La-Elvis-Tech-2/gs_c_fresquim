using Fresquim.Application.Abstractions;
using Fresquim.Domain.Avisos;
using Fresquim.Domain.Pontos;
using Microsoft.EntityFrameworkCore;

namespace Fresquim.Infrastructure.Persistence;

public class FresquimDbContext(DbContextOptions<FresquimDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<PontoApoio> Pontos => Set<PontoApoio>();
    public DbSet<Aviso> Avisos => Set<Aviso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as classes IEntityTypeConfiguration deste projeto.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FresquimDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // O SQLite não guarda o "tipo" da data: todas as datas são gravadas e lidas como UTC.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    public Task<int> SalvarAsync(CancellationToken ct = default) => SaveChangesAsync(ct);
}
