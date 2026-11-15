namespace Fresquim.Domain.Clima;

/// <summary>Resultado da classificação: nível, rótulo e orientação para o cidadão.</summary>
public sealed record RiscoCalor(NivelRisco Nivel, string Rotulo, string Orientacao)
{
    public const double LimiteAtencao = 30;
    public const double LimitePerigo = 35;

    /// <summary>
    /// Regra do Fresquim (a mesma do app): abaixo de 30 °C risco baixo,
    /// de 30 °C a 34 °C atenção, 35 °C ou mais perigo.
    /// </summary>
    public static RiscoCalor Classificar(CondicaoClimatica condicao)
    {
        var referencia = condicao.TemperaturaDeReferencia;

        return referencia switch
        {
            >= LimitePerigo => new(NivelRisco.Perigo, "Perigo",
                "Evite o sol das 10h às 16h, beba água a cada 20 minutos e procure sombra."),
            >= LimiteAtencao => new(NivelRisco.Atencao, "Atenção",
                "Beba água com frequência e faça pausas na sombra."),
            _ => new(NivelRisco.Baixo, "Risco baixo",
                "Dia tranquilo. Mesmo assim, leve uma garrafa de água."),
        };
    }
}
