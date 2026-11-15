using Fresquim.Domain.Common;

namespace Fresquim.Domain.Pontos;

public class Estacao : PontoApoio
{
    /// <summary>Linha de metrô ou trem (ex.: "Linha 2-Verde").</summary>
    public string Linha { get; private set; } = null!;

    public override TipoPonto Tipo => TipoPonto.Estacao;

    public override string Dica => $"Estação da {Linha}: área coberta e ventilada para se proteger do sol.";

    private Estacao() { }

    public Estacao(string nome, Endereco endereco, Coordenada localizacao, string horario, string linha)
        : base(nome, endereco, localizacao, horario)
    {
        DefinirLinha(linha);
    }

    public void DefinirLinha(string linha) => Linha = Guard.Texto(linha, "Linha", 60);
}
