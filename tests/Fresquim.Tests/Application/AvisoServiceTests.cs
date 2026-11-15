using Fresquim.Application.Abstractions;
using Fresquim.Application.Avisos;
using Fresquim.Application.Common;
using Fresquim.Domain.Avisos;
using Fresquim.Domain.Common;
using Fresquim.Domain.Pontos;
using Fresquim.Tests.Helpers;
using Moq;

namespace Fresquim.Tests.Application;

public class AvisoServiceTests
{
    private readonly Mock<IAvisoRepository> _avisos = new();
    private readonly Mock<IPontoApoioRepository> _pontos = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly AvisoService _service;

    public AvisoServiceTests()
    {
        _service = new AvisoService(_avisos.Object, _pontos.Object, _uow.Object);
    }

    [Fact]
    public async Task ReportarProblema_ComPontoInexistente_DeveLancarNaoEncontrado()
    {
        _pontos.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((PontoApoio?)null);

        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() =>
            _service.ReportarProblemaAsync(new ReportarProblemaRequest(Guid.NewGuid(), "Sem água desde ontem")));

        _uow.Verify(u => u.SalvarAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReportarProblema_ComAvisoJaAberto_DeveLancarDomainException()
    {
        var bebedouro = PontosFake.Bebedouro();
        _pontos.Setup(r => r.ObterPorIdAsync(bebedouro.Id, It.IsAny<CancellationToken>())).ReturnsAsync(bebedouro);
        _avisos.Setup(r => r.PontoTemAvisoAbertoAsync(bebedouro.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var erro = await Assert.ThrowsAsync<DomainException>(() =>
            _service.ReportarProblemaAsync(new ReportarProblemaRequest(bebedouro.Id, "Sem água desde ontem")));

        Assert.Contains("em aberto", erro.Message);
        _avisos.Verify(r => r.AdicionarAsync(It.IsAny<Aviso>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReportarProblema_Valido_DeveSalvarERetornarAvisoAberto()
    {
        var bebedouro = PontosFake.Bebedouro();
        _pontos.Setup(r => r.ObterPorIdAsync(bebedouro.Id, It.IsAny<CancellationToken>())).ReturnsAsync(bebedouro);
        _avisos.Setup(r => r.PontoTemAvisoAbertoAsync(bebedouro.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var resposta = await _service.ReportarProblemaAsync(new ReportarProblemaRequest(bebedouro.Id, "Sem água desde ontem"));

        Assert.Equal(StatusAviso.Aberto, resposta.Status);
        Assert.Equal(bebedouro.Nome, resposta.NomePonto);
        Assert.Equal(StatusPonto.ComProblema, bebedouro.Status);
        _avisos.Verify(r => r.AdicionarAsync(It.Is<Aviso>(a => a.PontoApoioId == bebedouro.Id), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SalvarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Resolver_AvisoInexistente_DeveLancarNaoEncontrado()
    {
        _avisos.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Aviso?)null);

        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() => _service.ResolverAsync(Guid.NewGuid(), "ok"));
    }

    [Fact]
    public async Task IniciarAnalise_DeveMudarStatusESalvar()
    {
        var aviso = Aviso.SugerirPonto("Praça do MASP", "Tem árvores e bancos na sombra");
        _avisos.Setup(r => r.ObterPorIdAsync(aviso.Id, It.IsAny<CancellationToken>())).ReturnsAsync(aviso);

        var resposta = await _service.IniciarAnaliseAsync(aviso.Id);

        Assert.Equal(StatusAviso.EmAnalise, resposta.Status);
        _uow.Verify(u => u.SalvarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
