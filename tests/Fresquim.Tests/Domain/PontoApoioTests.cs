using Fresquim.Domain.Clima;
using Fresquim.Domain.Common;
using Fresquim.Domain.Pontos;
using Fresquim.Tests.Helpers;

namespace Fresquim.Tests.Domain;

public class PontoApoioTests
{
    [Fact]
    public void CadaSubclasse_DeveInformarSeuTipo()
    {
        PontoApoio[] pontos = [PontosFake.Bebedouro(), PontosFake.Sombra(), PontosFake.Posto(), PontosFake.Estacao()];

        Assert.Equal(
            [TipoPonto.Bebedouro, TipoPonto.Sombra, TipoPonto.PostoSaude, TipoPonto.Estacao],
            pontos.Select(p => p.Tipo));
    }

    [Fact]
    public void Dica_DoBebedouro_MudaSeTemAguaGelada()
    {
        Assert.Contains("gelada", PontosFake.Bebedouro(gelada: true).Dica);
        Assert.DoesNotContain("gelada", PontosFake.Bebedouro(gelada: false).Dica);
    }

    [Fact]
    public void Dica_DaEstacao_IncluiALinha()
    {
        Assert.Contains("Linha 2-Verde", PontosFake.Estacao().Dica);
    }

    [Theory]
    [InlineData(NivelRisco.Baixo, false)]
    [InlineData(NivelRisco.Atencao, true)]
    [InlineData(NivelRisco.Perigo, true)]
    public void PostoSaude_SoERecomendadoAPartirDeAtencao(NivelRisco nivel, bool esperado)
    {
        Assert.Equal(esperado, PontosFake.Posto().RecomendadoPara(nivel));
    }

    [Fact]
    public void Sombra_PequenaNaoERecomendadaEmPerigo()
    {
        Assert.True(PontosFake.Sombra(capacidade: 5).RecomendadoPara(NivelRisco.Atencao));
        Assert.False(PontosFake.Sombra(capacidade: 5).RecomendadoPara(NivelRisco.Perigo));
        Assert.True(PontosFake.Sombra(capacidade: 50).RecomendadoPara(NivelRisco.Perigo));
    }

    [Fact]
    public void PontoComProblema_NaoERecomendado()
    {
        var bebedouro = PontosFake.Bebedouro();
        bebedouro.ReportarProblema();

        Assert.False(bebedouro.EstaDisponivel);
        Assert.False(bebedouro.RecomendadoPara(NivelRisco.Perigo));
    }

    [Fact]
    public void ReportarProblema_EmPontoDesativado_DeveLancarDomainException()
    {
        var estacao = PontosFake.Estacao();
        estacao.Desativar();

        Assert.Throws<DomainException>(() => estacao.ReportarProblema());
    }

    [Fact]
    public void MarcarComoFuncionando_DeveVoltarAEstarDisponivel()
    {
        var bebedouro = PontosFake.Bebedouro();
        bebedouro.ReportarProblema();

        bebedouro.MarcarComoFuncionando();

        Assert.Equal(StatusPonto.Funcionando, bebedouro.Status);
        Assert.NotNull(bebedouro.AtualizadoEm);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ComNomeVazio_DeveLancarDomainException(string nome)
    {
        Assert.Throws<DomainException>(() => PontosFake.Bebedouro(nome: nome));
    }

    [Fact]
    public void Sombra_ComCapacidadeZero_DeveLancarDomainException()
    {
        Assert.Throws<DomainException>(() => PontosFake.Sombra(capacidade: 0));
    }
}
