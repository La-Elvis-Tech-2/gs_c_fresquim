using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.WebUtilities;

namespace Fresquim.Api.Infrastructure;

/// <summary>
/// Traduz para português as mensagens que o próprio ASP.NET gera (binding, JSON inválido e títulos de erro).
/// As mensagens das regras de negócio e dos atributos de validação já são escritas em português.
/// </summary>
public static class MensagensPtBr
{
    private const string ValorInvalido = "Valor inválido.";

    private static readonly Dictionary<int, string> Titulos = new()
    {
        [StatusCodes.Status400BadRequest] = "Requisição inválida",
        [StatusCodes.Status404NotFound] = "Recurso não encontrado",
        [StatusCodes.Status405MethodNotAllowed] = "Método não permitido",
        [StatusCodes.Status406NotAcceptable] = "Formato de resposta não aceito",
        [StatusCodes.Status415UnsupportedMediaType] = "Tipo de conteúdo não suportado",
        [StatusCodes.Status500InternalServerError] = "Erro inesperado",
        [StatusCodes.Status503ServiceUnavailable] = "Serviço indisponível",
    };

    /// <summary>Mensagens de binding (valores que não puderam ser convertidos ou que faltaram).</summary>
    public static void Configurar(DefaultModelBindingMessageProvider p)
    {
        p.SetMissingBindRequiredValueAccessor(campo => $"Informe um valor para {campo}.");
        p.SetMissingKeyOrValueAccessor(() => "Informe um valor.");
        p.SetMissingRequestBodyRequiredValueAccessor(() => "O corpo da requisição é obrigatório.");
        p.SetValueMustNotBeNullAccessor(_ => "Este campo não pode ser nulo.");
        p.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"O valor '{valor}' não é válido para {campo}.");
        p.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"O valor '{valor}' não é válido.");
        p.SetUnknownValueIsInvalidAccessor(campo => $"O valor informado não é válido para {campo}.");
        p.SetNonPropertyUnknownValueIsInvalidAccessor(() => ValorInvalido);
        p.SetValueIsInvalidAccessor(valor => $"O valor '{valor}' não é válido.");
        p.SetValueMustBeANumberAccessor(campo => $"O campo {campo} deve ser um número.");
        p.SetNonPropertyValueMustBeANumberAccessor(() => "O valor deve ser um número.");
    }

    /// <summary>Resposta 400 com os erros de validação de cada campo.</summary>
    public static IActionResult RespostaDeValidacao(ActionContext contexto)
    {
        var erros = contexto.ModelState
            .Where(e => e.Value is { Errors.Count: > 0 })
            .ToDictionary(
                e => NomeDoCampo(e.Key),
                e => e.Value!.Errors
                    .Select(erro => string.IsNullOrWhiteSpace(erro.ErrorMessage)
                        ? (e.Key == "$" ? "O corpo da requisição não é um JSON válido." : ValorInvalido)
                        : erro.ErrorMessage)
                    .ToArray());

        var problema = new ValidationProblemDetails(erros)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Um ou mais campos são inválidos",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Instance = contexto.HttpContext.Request.Path,
        };
        problema.Extensions["traceId"] = contexto.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(problema) { ContentTypes = { "application/problem+json" } };
    }

    /// <summary>Troca os títulos padrão em inglês ("Not Found", "Bad Request"...) pelos em português.</summary>
    public static void TraduzirTitulo(ProblemDetailsContext contexto)
    {
        var problema = contexto.ProblemDetails;
        if (problema.Status is not { } status || !Titulos.TryGetValue(status, out var titulo))
            return;

        var tituloPadrao = problema.Title is null || problema.Title == ReasonPhrases.GetReasonPhrase(status);
        if (tituloPadrao)
            problema.Title = titulo;
    }

    // Padroniza a chave do erro com o nome do campo no JSON: "$.tipo" e "Nome" viram "tipo" e "nome".
    private static string NomeDoCampo(string chave)
    {
        if (chave is "" or "$")
            return "corpo";
        if (chave.StartsWith("$.", StringComparison.Ordinal))
            chave = chave[2..];
        return char.ToLowerInvariant(chave[0]) + chave[1..];
    }
}
