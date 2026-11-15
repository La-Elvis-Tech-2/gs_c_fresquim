using Fresquim.Domain.Clima;

namespace Fresquim.Domain.Pontos;

public class PostoSaude : PontoApoio
{
    /// <summary>Se o posto atende casos de emergência (ex.: insolação).</summary>
    public bool AtendeEmergencia { get; private set; }

    public override TipoPonto Tipo => TipoPonto.PostoSaude;

    public override string Dica => AtendeEmergencia
        ? "Sentiu tontura, dor de cabeça forte ou enjoo? Procure atendimento aqui."
        : "Posto para orientação e hidratação. Em emergência, ligue 192.";

    private PostoSaude() { }

    public PostoSaude(string nome, Endereco endereco, Coordenada localizacao, string horario, bool atendeEmergencia)
        : base(nome, endereco, localizacao, horario)
    {
        AtendeEmergencia = atendeEmergencia;
    }

    public void DefinirAtendeEmergencia(bool atendeEmergencia) => AtendeEmergencia = atendeEmergencia;

    /// <summary>Postos de saúde só entram nas recomendações a partir do nível "Atenção".</summary>
    public override bool RecomendadoPara(NivelRisco nivel) =>
        base.RecomendadoPara(nivel) && nivel >= NivelRisco.Atencao;
}
