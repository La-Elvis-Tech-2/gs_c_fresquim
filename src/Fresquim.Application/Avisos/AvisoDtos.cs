using System.ComponentModel.DataAnnotations;
using Fresquim.Domain.Avisos;

namespace Fresquim.Application.Avisos;

public sealed record AvisoFiltro
{
    public TipoAviso? Tipo { get; init; }
    public StatusAviso? Status { get; init; }
    public Guid? PontoApoioId { get; init; }

    [Range(1, int.MaxValue)]
    public int Pagina { get; init; } = 1;

    [Range(1, 100)]
    public int TamanhoPagina { get; init; } = 20;
}

public sealed record ReportarProblemaRequest(
    [Required] Guid PontoApoioId,
    [Required, StringLength(280, MinimumLength = 5)] string Descricao);

public sealed record SugerirPontoRequest(
    [Required, StringLength(120, MinimumLength = 3)] string NomeLocal,
    [Required, StringLength(280, MinimumLength = 5)] string Descricao);

/// <summary>Resposta da prefeitura ao resolver ou descartar um aviso.</summary>
public sealed record FinalizarAvisoRequest([Required, StringLength(280, MinimumLength = 3)] string Resposta);

public sealed record AvisoResponse(
    Guid Id,
    TipoAviso Tipo,
    StatusAviso Status,
    string Descricao,
    Guid? PontoApoioId,
    string? NomePonto,
    string? NomeLocalSugerido,
    string? RespostaPrefeitura,
    DateTime CriadoEm,
    DateTime? ResolvidoEm);
