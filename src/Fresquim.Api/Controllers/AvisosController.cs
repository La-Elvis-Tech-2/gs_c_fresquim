using Fresquim.Application.Avisos;
using Fresquim.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Fresquim.Api.Controllers;

/// <summary>Avisos do cidadão: problemas em pontos existentes e sugestões de pontos novos.</summary>
[ApiController]
[Route("api/avisos")]
[Produces("application/json")]
public class AvisosController(IAvisoService service) : ControllerBase
{
    /// <summary>Lista os avisos (mais recentes primeiro) com filtros e paginação.</summary>
    [HttpGet]
    [ProducesResponseType<PaginaResultado<AvisoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaResultado<AvisoResponse>>> Listar([FromQuery] AvisoFiltro filtro, CancellationToken ct) =>
        Ok(await service.ListarAsync(filtro, ct));

    /// <summary>Busca um aviso pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<AvisoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvisoResponse>> Obter(Guid id, CancellationToken ct) =>
        Ok(await service.ObterAsync(id, ct));

    /// <summary>Avisa um problema em um ponto (o ponto passa para "ComProblema").</summary>
    [HttpPost("problemas")]
    [ProducesResponseType<AvisoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvisoResponse>> ReportarProblema(ReportarProblemaRequest request, CancellationToken ct)
    {
        var aviso = await service.ReportarProblemaAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = aviso.Id }, aviso);
    }

    /// <summary>Sugere um ponto novo de água ou sombra.</summary>
    [HttpPost("sugestoes")]
    [ProducesResponseType<AvisoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AvisoResponse>> SugerirPonto(SugerirPontoRequest request, CancellationToken ct)
    {
        var aviso = await service.SugerirPontoAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = aviso.Id }, aviso);
    }

    /// <summary>A prefeitura começa a analisar o aviso (Aberto → EmAnalise).</summary>
    [HttpPatch("{id:guid}/analise")]
    [ProducesResponseType<AvisoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvisoResponse>> IniciarAnalise(Guid id, CancellationToken ct) =>
        Ok(await service.IniciarAnaliseAsync(id, ct));

    /// <summary>Resolve o aviso. Se for um problema, o ponto volta a "Funcionando".</summary>
    [HttpPatch("{id:guid}/resolver")]
    [ProducesResponseType<AvisoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvisoResponse>> Resolver(Guid id, FinalizarAvisoRequest request, CancellationToken ct) =>
        Ok(await service.ResolverAsync(id, request.Resposta, ct));

    /// <summary>Descarta o aviso com um motivo (ex.: duplicado ou fora da região).</summary>
    [HttpPatch("{id:guid}/descartar")]
    [ProducesResponseType<AvisoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvisoResponse>> Descartar(Guid id, FinalizarAvisoRequest request, CancellationToken ct) =>
        Ok(await service.DescartarAsync(id, request.Resposta, ct));

    /// <summary>Apaga um aviso.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        await service.RemoverAsync(id, ct);
        return NoContent();
    }
}
