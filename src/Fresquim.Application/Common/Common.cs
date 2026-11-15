namespace Fresquim.Application.Common;

/// <summary>Página de resultados com os dados de paginação.</summary>
public sealed record PaginaResultado<T>(IReadOnlyList<T> Itens, int Pagina, int TamanhoPagina, int Total)
{
    public int TotalPaginas => TamanhoPagina == 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanhoPagina);

    public PaginaResultado<TDestino> Mapear<TDestino>(Func<T, TDestino> mapear) =>
        new(Itens.Select(mapear).ToList(), Pagina, TamanhoPagina, Total);
}

/// <summary>O recurso pedido não existe (a API devolve HTTP 404).</summary>
public class RecursoNaoEncontradoException(string recurso, Guid id)
    : Exception($"{recurso} com id {id} não foi encontrado.");

/// <summary>Falha ao consultar um serviço externo (a API devolve HTTP 503).</summary>
public class ServicoExternoIndisponivelException(string mensagem, Exception? inner = null)
    : Exception(mensagem, inner);
