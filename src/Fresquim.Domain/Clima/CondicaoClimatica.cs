namespace Fresquim.Domain.Clima;

/// <summary>Leitura do tempo em um momento (vinda da API Open-Meteo).</summary>
public sealed record CondicaoClimatica(
    double Temperatura,
    double SensacaoTermica,
    double MaximaDoDia,
    double IndiceUv,
    DateTime MedidoEm)
{
    /// <summary>A maior medida entre temperatura e sensação é a que vale para o risco.</summary>
    public double TemperaturaDeReferencia => Math.Max(Temperatura, SensacaoTermica);
}
