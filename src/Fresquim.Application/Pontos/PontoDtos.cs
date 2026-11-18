using System.ComponentModel.DataAnnotations;
using Fresquim.Application.Common;
using Fresquim.Domain.Pontos;

namespace Fresquim.Application.Pontos;

/// <summary>Filtros da listagem de pontos (vêm da query string).</summary>
public sealed record PontoFiltro
{
    public TipoPonto? Tipo { get; init; }
    public StatusPonto? Status { get; init; }
    public string? Bairro { get; init; }

    /// <summary>Busca no nome ou no endereço.</summary>
    public string? Busca { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = Mensagens.PaginaMinima)]
    public int Pagina { get; init; } = 1;

    [Range(1, 100, ErrorMessage = Mensagens.Faixa)]
    public int TamanhoPagina { get; init; } = 20;
}

/// <summary>
/// Dados para cadastrar ou editar um ponto. Os campos do fim só valem para o tipo correspondente.
/// </summary>
public sealed record SalvarPontoRequest
{
    [Required(ErrorMessage = Mensagens.Obrigatorio)] public TipoPonto Tipo { get; init; }
    [Required(ErrorMessage = Mensagens.Obrigatorio), StringLength(120, MinimumLength = 3, ErrorMessage = Mensagens.TamanhoEntre)] public string Nome { get; init; } = "";
    [Required(ErrorMessage = Mensagens.Obrigatorio), StringLength(150, ErrorMessage = Mensagens.TamanhoMaximo)] public string Logradouro { get; init; } = "";
    [Required(ErrorMessage = Mensagens.Obrigatorio), StringLength(80, ErrorMessage = Mensagens.TamanhoMaximo)] public string Bairro { get; init; } = "";
    [Range(-90, 90, ErrorMessage = Mensagens.Faixa)] public double Latitude { get; init; }
    [Range(-180, 180, ErrorMessage = Mensagens.Faixa)] public double Longitude { get; init; }
    [Required(ErrorMessage = Mensagens.Obrigatorio), StringLength(80, ErrorMessage = Mensagens.TamanhoMaximo)] public string Horario { get; init; } = "";

    /// <summary>Bebedouro: tem água gelada?</summary>
    public bool? AguaGelada { get; init; }

    /// <summary>Sombra: quantas pessoas cabem.</summary>
    [Range(1, 10000, ErrorMessage = Mensagens.Faixa)] public int? CapacidadePessoas { get; init; }

    /// <summary>Posto de saúde: atende emergência?</summary>
    public bool? AtendeEmergencia { get; init; }

    /// <summary>Estação: linha de metrô/trem.</summary>
    [StringLength(60, ErrorMessage = Mensagens.TamanhoMaximo)] public string? Linha { get; init; }
}

public sealed record AlterarStatusPontoRequest([Required(ErrorMessage = Mensagens.Obrigatorio)] StatusPonto Status);

public sealed record PontoResponse(
    Guid Id,
    string Nome,
    TipoPonto Tipo,
    StatusPonto Status,
    string Logradouro,
    string Bairro,
    double Latitude,
    double Longitude,
    string Horario,
    string Dica,
    IReadOnlyDictionary<string, object> Detalhes,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record PontoProximoResponse(PontoResponse Ponto, double DistanciaKm);
