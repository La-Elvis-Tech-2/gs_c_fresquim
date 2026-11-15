using Fresquim.Domain.Clima;
using Fresquim.Domain.Common;

namespace Fresquim.Domain.Pontos;

/// <summary>
/// Ponto onde o cidadão encontra alívio do calor. Classe abstrata: cada tipo concreto
/// (bebedouro, sombra, posto de saúde, estação) define a própria dica e quando é recomendado.
/// </summary>
public abstract class PontoApoio : Entidade
{
    public string Nome { get; private set; } = null!;
    public Endereco Endereco { get; private set; } = null!;
    public Coordenada Localizacao { get; private set; } = null!;
    public string Horario { get; private set; } = null!;
    public StatusPonto Status { get; private set; } = StatusPonto.Funcionando;

    public abstract TipoPonto Tipo { get; }

    /// <summary>Orientação curta mostrada ao cidadão.</summary>
    public abstract string Dica { get; }

    protected PontoApoio() { } // usado pelo EF Core

    protected PontoApoio(string nome, Endereco endereco, Coordenada localizacao, string horario)
    {
        AtualizarDados(nome, endereco, localizacao, horario);
    }

    public bool EstaDisponivel => Status == StatusPonto.Funcionando;

    /// <summary>
    /// Indica se o ponto deve ser recomendado no nível de risco informado.
    /// Por padrão, qualquer ponto disponível ajuda; os tipos concretos podem refinar a regra.
    /// </summary>
    public virtual bool RecomendadoPara(NivelRisco nivel) => EstaDisponivel;

    public void AtualizarDados(string nome, Endereco endereco, Coordenada localizacao, string horario)
    {
        Nome = Guard.Texto(nome, "Nome", 120);
        Endereco = endereco ?? throw new DomainException("Endereço é obrigatório.");
        Localizacao = localizacao ?? throw new DomainException("Localização é obrigatória.");
        Horario = Guard.Texto(horario, "Horário", 80);
        MarcarAtualizacao();
    }

    public void ReportarProblema()
    {
        if (Status == StatusPonto.Desativado)
            throw new DomainException("Não é possível reportar problema em um ponto desativado.");

        Status = StatusPonto.ComProblema;
        MarcarAtualizacao();
    }

    public void MarcarComoFuncionando()
    {
        Status = StatusPonto.Funcionando;
        MarcarAtualizacao();
    }

    public void Desativar()
    {
        Status = StatusPonto.Desativado;
        MarcarAtualizacao();
    }

    public double DistanciaKm(Coordenada origem) => Localizacao.DistanciaKm(origem);
}
