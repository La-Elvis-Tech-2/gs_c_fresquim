using System.ComponentModel.DataAnnotations;
using Fresquim.Application.Common;
using Fresquim.Domain.Avisos;

namespace Fresquim.Application.Avisos;

public sealed record AvisoFiltro
{
    public TipoAviso? Tipo { get; init; }
    public StatusAviso? Status { get; init; }
    public Guid? PontoApoioId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = Mensagens.PaginaMinima)]
    public int Pagina { get; init; } = 1;

    [Range(1, 100, ErrorMessage = Mensagens.Faixa)]
    public int TamanhoPagina { get; init; } = 20;
}

public sealed record ReportarProblemaRequest(
    [Required(ErrorMessage = Mensagens.Obrigatorio)] Guid PontoApoioId,
    [Required(ErrorMessage = Mensagens.Obrigatorio), StringLength(280, MinimumLength = 5, ErrorMessage = Mensagens.TamanhoEntre)] string Descricao);

public sealed record SugerirPontoRequest(
    [Required(ErrorMessage = Mensagens.Obrigatorio), StringLength(120, MinimumLength = 3, ErrorMessage = Mensagens.TamanhoEntre)] string NomeLocal,
    [Required(ErrorMessage = Mensagens.Obrigatorio), StringLength(280, MinimumLength = 5, ErrorMessage = Mensagens.TamanhoEntre)] string Descricao);

/// <summary>Resposta da prefeitura ao resolver ou descartar um aviso.</summary>
public sealed record FinalizarAvisoRequest([Required(ErrorMessage = Mensagens.Obrigatorio), StringLength(280, MinimumLength = 3, ErrorMessage = Mensagens.TamanhoEntre)] string Resposta);

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
