using Fresquim.Application.Abstractions;
using Fresquim.Application.Clima;
using Fresquim.Domain.Clima;
using Fresquim.Domain.Pontos;
using Fresquim.Tests.Helpers;
using Moq;

namespace Fresquim.Tests.Application;

public class ClimaServiceTests
{
    private readonly Mock<IClimaProvider> _clima = new();
    private readonly Mock<IPontoApoioRepository> _pontos = new();

    private ClimaService CriarServico(double temperatura, params PontoApoio[] pontos)
    {
        _clima.Setup(c => c.ObterCondicaoAtualAsync(It.IsAny<Coordenada>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(new CondicaoClimatica(temperatura, temperatura, temperatura + 2, 8, DateTime.UtcNow));
        _pontos.Setup(r => r.ListarNaRegiaoAsync(It.IsAny<Coordenada>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(pontos);
        return new ClimaService(_clima.Object, _pontos.Object);
    }

    [Fact]
    public async Task AvaliarRisco_ComCalorExtremo_DeveRetornarPerigoERecomendarPostoDeSaude()
    {
        var service = CriarServico(37, PontosFake.Posto(), PontosFake.Bebedouro());

        var resposta = await service.AvaliarRiscoAsync(PontosFake.Paulista.Latitude, PontosFake.Paulista.Longitude, raioKm: 5);

        Assert.Equal(NivelRisco.Perigo, resposta.Nivel);
        Assert.Contains(resposta.PontosRecomendados, p => p.Ponto.Tipo == TipoPonto.PostoSaude);
    }

    [Fact]
    public async Task AvaliarRisco_ComRiscoBaixo_NaoDeveRecomendarPostoDeSaude()
    {
        var service = CriarServico(24, PontosFake.Posto(), PontosFake.Bebedouro());

        var resposta = await service.AvaliarRiscoAsync(PontosFake.Paulista.Latitude, PontosFake.Paulista.Longitude, raioKm: 5);

        Assert.Equal(NivelRisco.Baixo, resposta.Nivel);
        Assert.DoesNotContain(resposta.PontosRecomendados, p => p.Ponto.Tipo == TipoPonto.PostoSaude);
        Assert.Single(resposta.PontosRecomendados);
    }

    [Fact]
    public async Task AvaliarRisco_DeveOrdenarRecomendadosPelaDistanciaEIgnorarPontosForaDoRaio()
    {
        var perto = PontosFake.Bebedouro("Perto", -23.5615, -46.6560);
        var medio = PontosFake.Bebedouro("Médio", -23.5650, -46.6600);
        var longe = PontosFake.Bebedouro("Longe", -23.6500, -46.7500);
        var service = CriarServico(31, longe, medio, perto);

        var resposta = await service.AvaliarRiscoAsync(PontosFake.Paulista.Latitude, PontosFake.Paulista.Longitude, raioKm: 2);

        Assert.Equal(["Perto", "Médio"], resposta.PontosRecomendados.Select(p => p.Ponto.Nome));
        Assert.True(resposta.PontosRecomendados[0].DistanciaKm <= resposta.PontosRecomendados[1].DistanciaKm);
    }
}
