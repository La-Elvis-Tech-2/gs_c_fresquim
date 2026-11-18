using System.ComponentModel.DataAnnotations;
using Fresquim.Application.Common;
using Fresquim.Application.Pontos;
using Fresquim.Domain.Pontos;
using Microsoft.AspNetCore.Mvc;

namespace Fresquim.Api.Controllers;

/// <summary>Pontos de apoio contra o calor: bebedouros, sombras, postos de saúde e estações.</summary>
[ApiController]
[Route("api/pontos")]
[Produces("application/json")]
public class PontosController(IPontoApoioService service) : ControllerBase
{
    /// <summary>Lista os pontos com filtros e paginação.</summary>
    /// <remarks>Exemplo: <c>GET /api/pontos?tipo=Bebedouro&amp;bairro=Bela Vista&amp;pagina=1</c></remarks>
    [HttpGet]
    [ProducesResponseType<PaginaResultado<PontoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaResultado<PontoResponse>>> Listar([FromQuery] PontoFiltro filtro, CancellationToken ct) =>
        Ok(await service.ListarAsync(filtro, ct));

    /// <summary>Busca um ponto pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PontoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PontoResponse>> Obter(Guid id, CancellationToken ct) =>
        Ok(await service.ObterAsync(id, ct));

    /// <summary>Pontos disponíveis mais próximos de uma coordenada, ordenados pela distância.</summary>
    /// <param name="latitude">Latitude de quem está procurando.</param>
    /// <param name="longitude">Longitude de quem está procurando.</param>
    /// <param name="raioKm">Raio de busca em km (padrão 2 km, máximo 20 km).</param>
    /// <param name="tipo">Filtra por tipo de ponto (opcional).</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    [HttpGet("proximos")]
    [ProducesResponseType<IReadOnlyList<PontoProximoResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PontoProximoResponse>>> Proximos(
        [FromQuery, Required(ErrorMessage = Mensagens.Obrigatorio), Range(-90, 90, ErrorMessage = Mensagens.Faixa)] double latitude,
        [FromQuery, Required(ErrorMessage = Mensagens.Obrigatorio), Range(-180, 180, ErrorMessage = Mensagens.Faixa)] double longitude,
        [FromQuery, Range(0.1, 20, ErrorMessage = Mensagens.Faixa)] double raioKm = 2,
        [FromQuery] TipoPonto? tipo = null,
        CancellationToken ct = default) =>
        Ok(await service.ListarProximosAsync(latitude, longitude, raioKm, tipo, ct));

    /// <summary>Cadastra um ponto novo.</summary>
    /// <remarks>
    /// Campos específicos por tipo: <c>aguaGelada</c> (Bebedouro), <c>capacidadePessoas</c> (Sombra, obrigatório),
    /// <c>atendeEmergencia</c> (PostoSaude) e <c>linha</c> (Estacao, obrigatório).
    /// </remarks>
    [HttpPost]
    [ProducesResponseType<PontoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PontoResponse>> Criar(SalvarPontoRequest request, CancellationToken ct)
    {
        var ponto = await service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = ponto.Id }, ponto);
    }

    /// <summary>Edita os dados de um ponto (o tipo não pode mudar).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<PontoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PontoResponse>> Atualizar(Guid id, SalvarPontoRequest request, CancellationToken ct) =>
        Ok(await service.AtualizarAsync(id, request, ct));

    /// <summary>Muda o status do ponto (Funcionando, ComProblema ou Desativado).</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType<PontoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PontoResponse>> AlterarStatus(Guid id, AlterarStatusPontoRequest request, CancellationToken ct) =>
        Ok(await service.AlterarStatusAsync(id, request.Status, ct));

    /// <summary>Remove um ponto. Os avisos ligados a ele continuam no histórico.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        await service.RemoverAsync(id, ct);
        return NoContent();
    }
}
