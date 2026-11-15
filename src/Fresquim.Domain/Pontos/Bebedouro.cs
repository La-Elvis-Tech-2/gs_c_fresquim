namespace Fresquim.Domain.Pontos;

public class Bebedouro : PontoApoio
{
    /// <summary>Se o bebedouro tem água gelada.</summary>
    public bool AguaGelada { get; private set; }

    public override TipoPonto Tipo => TipoPonto.Bebedouro;

    public override string Dica => AguaGelada
        ? "Água gelada disponível. Encha sua garrafa e beba aos poucos."
        : "Leve uma garrafa para encher e beba água mesmo sem sede.";

    private Bebedouro() { }

    public Bebedouro(string nome, Endereco endereco, Coordenada localizacao, string horario, bool aguaGelada)
        : base(nome, endereco, localizacao, horario)
    {
        AguaGelada = aguaGelada;
    }

    public void DefinirAguaGelada(bool aguaGelada) => AguaGelada = aguaGelada;
}
