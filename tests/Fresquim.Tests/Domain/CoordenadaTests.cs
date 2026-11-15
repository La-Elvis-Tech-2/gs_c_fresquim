using Fresquim.Domain.Common;
using Fresquim.Domain.Pontos;

namespace Fresquim.Tests.Domain;

public class CoordenadaTests
{
    [Fact]
    public void DistanciaKm_EntreMesmoPonto_DeveSerZero()
    {
        var c = new Coordenada(-23.5614, -46.6559);

        Assert.Equal(0, c.DistanciaKm(c), precision: 6);
    }

    [Fact]
    public void DistanciaKm_EntreTrianonEConsolacao_DeveSerCercaDe650Metros()
    {
        var trianon = new Coordenada(-23.5614, -46.6559);
        var consolacao = new Coordenada(-23.5577, -46.6608);

        var distancia = trianon.DistanciaKm(consolacao);

        Assert.InRange(distancia, 0.6, 0.7);
    }

    [Fact]
    public void DistanciaKm_DeveSerSimetrica()
    {
        var a = new Coordenada(-23.55, -46.63);
        var b = new Coordenada(-23.60, -46.70);

        Assert.Equal(a.DistanciaKm(b), b.DistanciaKm(a), precision: 9);
    }

    [Theory]
    [InlineData(91, 0)]
    [InlineData(-91, 0)]
    [InlineData(0, 181)]
    [InlineData(0, -181)]
    public void Construtor_ComValorForaDoLimite_DeveLancarDomainException(double lat, double lon)
    {
        Assert.Throws<DomainException>(() => new Coordenada(lat, lon));
    }

    [Fact]
    public void Coordenadas_ComMesmosValores_SaoIguais()
    {
        Assert.Equal(new Coordenada(-23.5, -46.6), new Coordenada(-23.5, -46.6));
    }
}
