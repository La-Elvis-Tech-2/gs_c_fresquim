using Fresquim.Domain.Common;
using Fresquim.Domain.Pontos;

namespace Fresquim.Domain.Avisos;

public enum TipoAviso
{
    /// <summary>Algo não funciona (ex.: bebedouro sem água).</summary>
    Problema = 1,

    /// <summary>Sugestão de um ponto novo.</summary>
    Sugestao = 2,
}

public enum StatusAviso
{
    Aberto = 1,
    EmAnalise = 2,
    Resolvido = 3,
    Descartado = 4,
}

/// <summary>
/// Aviso enviado pelo cidadão. O status só muda por métodos que validam a transição
/// (Aberto → EmAnalise → Resolvido/Descartado).
/// </summary>
public class Aviso : Entidade
{
    public TipoAviso Tipo { get; private set; }
    public string Descricao { get; private set; } = null!;
    public StatusAviso Status { get; private set; } = StatusAviso.Aberto;
    public Guid? PontoApoioId { get; private set; }
    public PontoApoio? PontoApoio { get; private set; }
    public string? NomeLocalSugerido { get; private set; }
    public string? RespostaPrefeitura { get; private set; }
    public DateTime? ResolvidoEm { get; private set; }

    private Aviso() { } // usado pelo EF Core

    private Aviso(TipoAviso tipo, string descricao)
    {
        Tipo = tipo;
        Descricao = Guard.Texto(descricao, "Descrição", 280);
        if (Descricao.Length < 5)
            throw new DomainException("Descrição deve ter pelo menos 5 caracteres.");
    }

    /// <summary>Cria um aviso de problema ligado a um ponto existente e marca o ponto com problema.</summary>
    public static Aviso ReportarProblema(PontoApoio ponto, string descricao)
    {
        ArgumentNullException.ThrowIfNull(ponto);

        var aviso = new Aviso(TipoAviso.Problema, descricao)
        {
            PontoApoioId = ponto.Id,
            PontoApoio = ponto,
        };
        ponto.ReportarProblema();
        return aviso;
    }

    /// <summary>Cria uma sugestão de ponto novo (ainda sem cadastro).</summary>
    public static Aviso SugerirPonto(string nomeLocal, string descricao) =>
        new(TipoAviso.Sugestao, descricao)
        {
            NomeLocalSugerido = Guard.Texto(nomeLocal, "Nome do local", 120),
        };

    public bool EstaFinalizado => Status is StatusAviso.Resolvido or StatusAviso.Descartado;

    public void IniciarAnalise()
    {
        if (Status != StatusAviso.Aberto)
            throw new DomainException("Só avisos abertos podem entrar em análise.");

        Status = StatusAviso.EmAnalise;
        MarcarAtualizacao();
    }

    public void Resolver(string resposta)
    {
        Finalizar(StatusAviso.Resolvido, resposta);
        // Problema resolvido: o ponto volta a funcionar.
        if (Tipo == TipoAviso.Problema && PontoApoio is { Status: StatusPonto.ComProblema })
            PontoApoio.MarcarComoFuncionando();
    }

    public void Descartar(string motivo) => Finalizar(StatusAviso.Descartado, motivo);

    private void Finalizar(StatusAviso novoStatus, string resposta)
    {
        if (EstaFinalizado)
            throw new DomainException("Este aviso já foi finalizado.");

        RespostaPrefeitura = Guard.Texto(resposta, "Resposta", 280);
        Status = novoStatus;
        ResolvidoEm = DateTime.UtcNow;
        MarcarAtualizacao();
    }
}
