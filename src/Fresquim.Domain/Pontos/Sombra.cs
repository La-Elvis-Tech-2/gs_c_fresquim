using Fresquim.Domain.Clima;
using Fresquim.Domain.Common;

namespace Fresquim.Domain.Pontos;

public class Sombra : PontoApoio
{
    /// <summary>Quantas pessoas cabem na sombra ao mesmo tempo (estimativa).</summary>
    public int CapacidadePessoas { get; private set; }

    public override TipoPonto Tipo => TipoPonto.Sombra;

    public override string Dica => "Descanse alguns minutos na sombra nas horas mais quentes (10h às 16h).";

    private Sombra() { }

    public Sombra(string nome, Endereco endereco, Coordenada localizacao, string horario, int capacidadePessoas)
        : base(nome, endereco, localizacao, horario)
    {
        DefinirCapacidade(capacidadePessoas);
    }

    public void DefinirCapacidade(int capacidadePessoas)
    {
        if (capacidadePessoas <= 0) throw new DomainException("Capacidade deve ser maior que zero.");
        CapacidadePessoas = capacidadePessoas;
    }

    /// <summary>Em "Perigo", só recomenda sombras que comportam um grupo (10 pessoas ou mais).</summary>
    public override bool RecomendadoPara(NivelRisco nivel) =>
        base.RecomendadoPara(nivel) && (nivel != NivelRisco.Perigo || CapacidadePessoas >= 10);
}
