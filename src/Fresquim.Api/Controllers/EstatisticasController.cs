using Fresquim.Application.Estatisticas;
using Microsoft.AspNetCore.Mvc;

namespace Fresquim.Api.Controllers;

/// <summary>Indicadores de impacto da CitizenTech.</summary>
[ApiController]
[Route("api/estatisticas")]
[Produces("application/json")]
public class EstatisticasController(IEstatisticasService service) : ControllerBase
{
    /// <summary>Totais de pontos e avisos, tempo médio de resolução e resumo por bairro.</summary>
    [HttpGet]
    [ProducesResponseType<EstatisticasResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EstatisticasResponse>> Obter(CancellationToken ct) =>
        Ok(await service.ObterAsync(ct));
}
