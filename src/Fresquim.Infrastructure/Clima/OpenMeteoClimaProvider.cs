using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Fresquim.Application.Abstractions;
using Fresquim.Application.Common;
using Fresquim.Domain.Clima;
using Fresquim.Domain.Pontos;

namespace Fresquim.Infrastructure.Clima;

/// <summary>
/// Consulta a API pública e gratuita Open-Meteo (sem chave) com HttpClient tipado.
/// Documentação: https://open-meteo.com/en/docs
/// </summary>
public class OpenMeteoClimaProvider(HttpClient http) : IClimaProvider
{
    public async Task<CondicaoClimatica> ObterCondicaoAtualAsync(Coordenada local, CancellationToken ct = default)
    {
        const string campos = "&current=temperature_2m,apparent_temperature,uv_index" +
                              "&daily=temperature_2m_max&timezone=America%2FSao_Paulo&forecast_days=1";
        var url = string.Create(CultureInfo.InvariantCulture,
            $"v1/forecast?latitude={local.Latitude}&longitude={local.Longitude}{campos}");

        try
        {
            var resposta = await http.GetFromJsonAsync<OpenMeteoResposta>(url, ct)
                           ?? throw new ServicoExternoIndisponivelException("A Open-Meteo devolveu uma resposta vazia.");

            return new CondicaoClimatica(
                Math.Round(resposta.Current.Temperature),
                Math.Round(resposta.Current.ApparentTemperature),
                Math.Round(resposta.Daily.TemperatureMax.FirstOrDefault()),
                Math.Round(resposta.Current.UvIndex ?? 0, 1),
                DateTime.UtcNow);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException
                                   && !ct.IsCancellationRequested)
        {
            throw new ServicoExternoIndisponivelException("Não foi possível consultar o tempo agora. Tente de novo em instantes.", ex);
        }
    }

    private sealed record OpenMeteoResposta(
        [property: JsonPropertyName("current")] AtualDto Current,
        [property: JsonPropertyName("daily")] DiarioDto Daily);

    private sealed record AtualDto(
        [property: JsonPropertyName("temperature_2m")] double Temperature,
        [property: JsonPropertyName("apparent_temperature")] double ApparentTemperature,
        [property: JsonPropertyName("uv_index")] double? UvIndex);

    private sealed record DiarioDto(
        [property: JsonPropertyName("temperature_2m_max")] double[] TemperatureMax);
}
