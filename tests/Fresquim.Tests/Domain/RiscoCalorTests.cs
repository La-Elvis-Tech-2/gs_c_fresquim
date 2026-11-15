using Fresquim.Domain.Clima;

namespace Fresquim.Tests.Domain;

public class RiscoCalorTests
{
    private static CondicaoClimatica Condicao(double temperatura, double sensacao) =>
        new(temperatura, sensacao, temperatura + 3, 5, DateTime.UtcNow);

    [Theory]
    [InlineData(22, 23, NivelRisco.Baixo)]
    [InlineData(29.9, 29, NivelRisco.Baixo)]
    [InlineData(30, 28, NivelRisco.Atencao)]
    [InlineData(34, 34.9, NivelRisco.Atencao)]
    [InlineData(35, 33, NivelRisco.Perigo)]
    [InlineData(40, 44, NivelRisco.Perigo)]
    public void Classificar_DeveUsarAsFaixasDoFresquim(double temperatura, double sensacao, NivelRisco esperado)
    {
        var risco = RiscoCalor.Classificar(Condicao(temperatura, sensacao));

        Assert.Equal(esperado, risco.Nivel);
    }

    [Fact]
    public void Classificar_DeveConsiderarASensacaoTermicaQuandoForMaior()
    {
        // 29 °C sozinho seria "Baixo", mas a sensação de 36 °C indica "Perigo".
        var risco = RiscoCalor.Classificar(Condicao(29, 36));

        Assert.Equal(NivelRisco.Perigo, risco.Nivel);
        Assert.Equal("Perigo", risco.Rotulo);
        Assert.False(string.IsNullOrWhiteSpace(risco.Orientacao));
    }
}
