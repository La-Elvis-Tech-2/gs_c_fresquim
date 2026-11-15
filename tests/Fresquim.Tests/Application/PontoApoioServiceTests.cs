using Fresquim.Application.Abstractions;
using Fresquim.Application.Common;
using Fresquim.Application.Pontos;
using Fresquim.Domain.Common;
using Fresquim.Domain.Pontos;
using Fresquim.Tests.Helpers;
using Moq;

namespace Fresquim.Tests.Application;

public class PontoApoioServiceTests
{
    private readonly Mock<IPontoApoioRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly PontoApoioService _service;

    public PontoApoioServiceTests()
    {
        _service = new PontoApoioService(_repo.Object, _uow.Object);
    }

    private static SalvarPontoRequest Request(TipoPonto tipo) => new()
    {
        Tipo = tipo,
        Nome = "Ponto novo",
        Logradouro = "Avenida Paulista, 1000",
        Bairro = "Bela Vista",
        Latitude = -23.56,
        Longitude = -46.65,
        Horario = "24 horas",
        CapacidadePessoas = 20,
        Linha = "Linha 2-Verde",
    };

    [Theory]
    [InlineData(TipoPonto.Bebedouro, typeof(Bebedouro))]
    [InlineData(TipoPonto.Sombra, typeof(Sombra))]
    [InlineData(TipoPonto.PostoSaude, typeof(PostoSaude))]
    [InlineData(TipoPonto.Estacao, typeof(Estacao))]
    public async Task Criar_DeveInstanciarASubclasseCorreta(TipoPonto tipo, Type esperado)
    {
        var resposta = await _service.CriarAsync(Request(tipo));

        Assert.Equal(tipo, resposta.Tipo);
        _repo.Verify(r => r.AdicionarAsync(It.Is<PontoApoio>(p => p.GetType() == esperado), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SalvarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Criar_SombraSemCapacidade_DeveLancarDomainException()
    {
        var request = Request(TipoPonto.Sombra) with { CapacidadePessoas = null };

        await Assert.ThrowsAsync<DomainException>(() => _service.CriarAsync(request));
    }

    [Fact]
    public async Task Atualizar_TentandoMudarOTipo_DeveLancarDomainException()
    {
        var bebedouro = PontosFake.Bebedouro();
        _repo.Setup(r => r.ObterPorIdAsync(bebedouro.Id, It.IsAny<CancellationToken>())).ReturnsAsync(bebedouro);

        await Assert.ThrowsAsync<DomainException>(() => _service.AtualizarAsync(bebedouro.Id, Request(TipoPonto.Sombra)));
        _uow.Verify(u => u.SalvarAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Obter_PontoInexistente_DeveLancarNaoEncontrado()
    {
        _repo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PontoApoio?)null);

        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() => _service.ObterAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ListarProximos_DeveIgnorarIndisponiveisEOrdenarPorDistancia()
    {
        var perto = PontosFake.Bebedouro("Perto", -23.5615, -46.6560);
        var quebrado = PontosFake.Bebedouro("Quebrado", -23.5614, -46.6559);
        quebrado.ReportarProblema();
        var estacao = PontosFake.Estacao(); // ~0,65 km
        _repo.Setup(r => r.ListarNaRegiaoAsync(It.IsAny<Coordenada>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync([estacao, quebrado, perto]);

        var resultado = await _service.ListarProximosAsync(-23.5614, -46.6559, raioKm: 2, tipo: null);

        Assert.Equal(["Perto", "Estação Consolação"], resultado.Select(r => r.Ponto.Nome));
    }

    [Fact]
    public async Task ListarProximos_ComFiltroDeTipo_DeveRetornarSoAqueleTipo()
    {
        _repo.Setup(r => r.ListarNaRegiaoAsync(It.IsAny<Coordenada>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync([PontosFake.Estacao(), PontosFake.Bebedouro()]);

        var resultado = await _service.ListarProximosAsync(-23.5614, -46.6559, raioKm: 2, tipo: TipoPonto.Estacao);

        Assert.All(resultado, r => Assert.Equal(TipoPonto.Estacao, r.Ponto.Tipo));
        Assert.Single(resultado);
    }
}
