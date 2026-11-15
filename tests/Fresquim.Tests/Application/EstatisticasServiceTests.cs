using Fresquim.Application.Abstractions;
using Fresquim.Application.Estatisticas;
using Fresquim.Domain.Avisos;
using Fresquim.Domain.Pontos;
using Fresquim.Tests.Helpers;
using Moq;

namespace Fresquim.Tests.Application;

public class EstatisticasServiceTests
{
    [Fact]
    public async Task Obter_DeveAgruparPontosEAvisosCorretamente()
    {
        var bebedouro = PontosFake.Bebedouro();          // Bela Vista
        var posto = PontosFake.Posto();                  // Bela Vista
        var estacao = PontosFake.Estacao("Consolação");  // Consolação
        var sombra = PontosFake.Sombra();                // Paraíso

        var avisoAberto = Aviso.ReportarProblema(bebedouro, "Sem água desde ontem");
        var avisoResolvido = Aviso.ReportarProblema(estacao, "Escada rolante parada");
        avisoResolvido.Resolver("Consertado");

        var pontosRepo = new Mock<IPontoApoioRepository>();
        pontosRepo.Setup(r => r.ListarTodosAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync([bebedouro, posto, estacao, sombra]);
        var avisosRepo = new Mock<IAvisoRepository>();
        avisosRepo.Setup(r => r.ListarTodosAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync([avisoAberto, avisoResolvido]);

        var resultado = await new EstatisticasService(pontosRepo.Object, avisosRepo.Object).ObterAsync();

        Assert.Equal(4, resultado.TotalPontos);
        Assert.Equal(3, resultado.PontosDisponiveis); // o bebedouro está com problema
        Assert.Equal(75, resultado.PercentualDisponivel);
        Assert.Equal(2, resultado.TotalAvisos);
        Assert.Equal(1, resultado.AvisosResolvidos);
        Assert.NotNull(resultado.TempoMedioResolucaoHoras);

        var belaVista = Assert.Single(resultado.Bairros, b => b.Bairro == "Bela Vista");
        Assert.Equal(2, belaVista.TotalPontos);
        Assert.Equal(1, belaVista.PontosComProblema);
        Assert.Equal(1, belaVista.AvisosAbertos);

        // O bairro com mais avisos abertos aparece primeiro.
        Assert.Equal("Bela Vista", resultado.Bairros[0].Bairro);
        Assert.Contains(resultado.PontosPorTipo, c => c.Chave == nameof(TipoPonto.Sombra) && c.Total == 1);
    }

    [Fact]
    public async Task Obter_SemDados_DeveRetornarZerosSemErro()
    {
        var pontosRepo = new Mock<IPontoApoioRepository>();
        pontosRepo.Setup(r => r.ListarTodosAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var avisosRepo = new Mock<IAvisoRepository>();
        avisosRepo.Setup(r => r.ListarTodosAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var resultado = await new EstatisticasService(pontosRepo.Object, avisosRepo.Object).ObterAsync();

        Assert.Equal(0, resultado.TotalPontos);
        Assert.Equal(0, resultado.PercentualDisponivel);
        Assert.Null(resultado.TempoMedioResolucaoHoras);
        Assert.Empty(resultado.Bairros);
    }
}
