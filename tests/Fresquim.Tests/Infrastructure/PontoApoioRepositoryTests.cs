using Fresquim.Application.Pontos;
using Fresquim.Domain.Avisos;
using Fresquim.Domain.Pontos;
using Fresquim.Infrastructure.Persistence;
using Fresquim.Infrastructure.Persistence.Repositories;
using Fresquim.Tests.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Fresquim.Tests.Infrastructure;

/// <summary>Testa as consultas LINQ do repositório em um SQLite em memória (banco real, sem arquivo).</summary>
public sealed class PontoApoioRepositoryTests : IAsyncLifetime
{
    private readonly SqliteConnection _conexao = new("Data Source=:memory:");
    private FresquimDbContext _db = null!;

    public async Task InitializeAsync()
    {
        await _conexao.OpenAsync();
        _db = new FresquimDbContext(new DbContextOptionsBuilder<FresquimDbContext>().UseSqlite(_conexao).Options);
        await _db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(_db);
        _db.ChangeTracker.Clear();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _conexao.DisposeAsync();
    }

    [Fact]
    public async Task Seed_DeveCadastrarOsOitoPontosComAsSubclassesCertas()
    {
        var todos = await new PontoApoioRepository(_db).ListarTodosAsync();

        Assert.Equal(8, todos.Count);
        Assert.Equal(2, todos.OfType<Bebedouro>().Count());
        Assert.Equal(2, todos.OfType<Estacao>().Count());
        Assert.All(todos.OfType<Estacao>(), e => Assert.Equal("Linha 2-Verde", e.Linha));
    }

    [Fact]
    public async Task Listar_FiltrandoPorTipo_DeveUsarODiscriminador()
    {
        var pagina = await new PontoApoioRepository(_db).ListarAsync(new PontoFiltro { Tipo = TipoPonto.PostoSaude });

        Assert.Equal(2, pagina.Total);
        Assert.All(pagina.Itens, p => Assert.IsType<PostoSaude>(p));
    }

    [Fact]
    public async Task Listar_FiltrandoPorBairroEStatus_DeveCombinarOsFiltros()
    {
        var pagina = await new PontoApoioRepository(_db).ListarAsync(new PontoFiltro
        {
            Bairro = "consolação",
            Status = StatusPonto.ComProblema,
        });

        var ponto = Assert.Single(pagina.Itens);
        Assert.Equal("Bebedouro da Praça do Ciclista", ponto.Nome);
    }

    [Fact]
    public async Task Listar_ComBusca_DeveProcurarNoNomeENoEndereco()
    {
        var repo = new PontoApoioRepository(_db);

        var porNome = await repo.ListarAsync(new PontoFiltro { Busca = "Trianon" });
        var porEndereco = await repo.ListarAsync(new PontoFiltro { Busca = "Paulista" });

        Assert.Single(porNome.Itens);
        Assert.Equal(3, porEndereco.Total);
    }

    [Fact]
    public async Task Listar_DevePaginarEOrdenarPorNome()
    {
        var repo = new PontoApoioRepository(_db);

        var pagina1 = await repo.ListarAsync(new PontoFiltro { Pagina = 1, TamanhoPagina = 3 });
        var pagina3 = await repo.ListarAsync(new PontoFiltro { Pagina = 3, TamanhoPagina = 3 });

        Assert.Equal(8, pagina1.Total);
        Assert.Equal(3, pagina1.TotalPaginas);
        Assert.Equal(3, pagina1.Itens.Count);
        Assert.Equal(2, pagina3.Itens.Count);
        Assert.Equal(pagina1.Itens.Select(p => p.Nome).Order(), pagina1.Itens.Select(p => p.Nome));
    }

    [Fact]
    public async Task ListarNaRegiao_DeveDeixarDeForaPontosLonge()
    {
        // Raio de 1 km ao redor do Trianon: o Parque da Aclimação (~2,8 km) fica de fora.
        var naRegiao = await new PontoApoioRepository(_db).ListarNaRegiaoAsync(PontosFake.Paulista, raioKm: 1);

        Assert.Contains(naRegiao, p => p.Nome == "Bebedouro do Parque Trianon");
        Assert.DoesNotContain(naRegiao, p => p.Nome == "Parque da Aclimação (sombra)");
    }

    [Fact]
    public async Task Remover_PontoComAviso_DeveManterOAvisoSemVinculo()
    {
        var repo = new PontoApoioRepository(_db);
        var ponto = (await repo.ListarAsync(new PontoFiltro { Busca = "Trianon" })).Itens[0];
        var rastreado = await repo.ObterPorIdAsync(ponto.Id);
        _db.Avisos.Add(Aviso.ReportarProblema(rastreado!, "Sem água desde ontem"));
        await _db.SaveChangesAsync();

        repo.Remover(rastreado!);
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        var aviso = await _db.Avisos.SingleAsync();
        Assert.Null(aviso.PontoApoioId);
    }
}
