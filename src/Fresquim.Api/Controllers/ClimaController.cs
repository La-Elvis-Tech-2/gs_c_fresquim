using System.ComponentModel.DataAnnotations;
using Fresquim.Application.Clima;
using Microsoft.AspNetCore.Mvc;

namespace Fresquim.Api.Controllers;

/// <summary>Risco de calor agora, com base na API Open-Meteo.</summary>
[ApiController]
[Route("api/clima")]
[Produces("application/json")]
public class ClimaController(IClimaService service) : ControllerBase
{
    /// <summary>
    /// Temperatura, sensação térmica e nível de risco no local, com os pontos recomendados por perto.
    /// </summary>
    /// <param name="latitude">Latitude (padrão: Av. Paulista).</param>
    /// <param name="longitude">Longitude (padrão: Av. Paulista).</param>
    /// <param name="raioKm">Raio para buscar pontos recomendados (padrão 2 km).</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    [HttpGet("risco")]
    [ProducesResponseType<RiscoCalorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<RiscoCalorResponse>> Risco(
        [FromQuery, Range(-90, 90)] double latitude = -23.5614,
        [FromQuery, Range(-180, 180)] double longitude = -46.6559,
        [FromQuery, Range(0.1, 20)] double raioKm = 2,
        CancellationToken ct = default) =>
        Ok(await service.AvaliarRiscoAsync(latitude, longitude, raioKm, ct));
}
