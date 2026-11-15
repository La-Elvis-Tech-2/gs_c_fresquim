using Fresquim.Domain.Avisos;
using Fresquim.Domain.Common;
using Fresquim.Domain.Pontos;
using Fresquim.Tests.Helpers;

namespace Fresquim.Tests.Domain;

public class AvisoTests
{
    [Fact]
    public void ReportarProblema_DeveCriarAvisoAbertoEMarcarPontoComProblema()
    {
        var bebedouro = PontosFake.Bebedouro();

        var aviso = Aviso.ReportarProblema(bebedouro, "Sem água desde ontem");

        Assert.Equal(TipoAviso.Problema, aviso.Tipo);
        Assert.Equal(StatusAviso.Aberto, aviso.Status);
        Assert.Equal(bebedouro.Id, aviso.PontoApoioId);
        Assert.Equal(StatusPonto.ComProblema, bebedouro.Status);
    }

    [Fact]
    public void SugerirPonto_DeveGuardarONomeDoLocal()
    {
        var aviso = Aviso.SugerirPonto("Praça do MASP", "Tem árvores e bancos na sombra");

        Assert.Equal(TipoAviso.Sugestao, aviso.Tipo);
        Assert.Equal("Praça do MASP", aviso.NomeLocalSugerido);
        Assert.Null(aviso.PontoApoioId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("oi")]
    public void Descricao_MuitoCurta_DeveLancarDomainException(string descricao)
    {
        Assert.Throws<DomainException>(() => Aviso.SugerirPonto("Praça", descricao));
    }

    [Fact]
    public void Resolver_ProblemaDeveFazerOPontoVoltarAFuncionar()
    {
        var bebedouro = PontosFake.Bebedouro();
        var aviso = Aviso.ReportarProblema(bebedouro, "Sem água desde ontem");
        aviso.IniciarAnalise();

        aviso.Resolver("Consertado pela subprefeitura");

        Assert.Equal(StatusAviso.Resolvido, aviso.Status);
        Assert.NotNull(aviso.ResolvidoEm);
        Assert.Equal("Consertado pela subprefeitura", aviso.RespostaPrefeitura);
        Assert.Equal(StatusPonto.Funcionando, bebedouro.Status);
    }

    [Fact]
    public void IniciarAnalise_DuasVezes_DeveLancarDomainException()
    {
        var aviso = Aviso.SugerirPonto("Praça", "Boa sombra para descansar");
        aviso.IniciarAnalise();

        Assert.Throws<DomainException>(() => aviso.IniciarAnalise());
    }

    [Fact]
    public void AvisoFinalizado_NaoPodeSerResolvidoNemDescartadoDeNovo()
    {
        var aviso = Aviso.SugerirPonto("Praça", "Boa sombra para descansar");
        aviso.Descartar("Fora da região atendida");

        Assert.True(aviso.EstaFinalizado);
        Assert.Throws<DomainException>(() => aviso.Resolver("ok"));
        Assert.Throws<DomainException>(() => aviso.Descartar("de novo"));
    }
}
