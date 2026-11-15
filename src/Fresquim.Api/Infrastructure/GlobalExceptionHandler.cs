using Fresquim.Application.Common;
using Fresquim.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fresquim.Api.Infrastructure;

/// <summary>Converte as exceções da aplicação em respostas ProblemDetails (RFC 9457).</summary>
public class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, titulo) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Regra de negócio violada"),
            RecursoNaoEncontradoException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            ServicoExternoIndisponivelException => (StatusCodes.Status503ServiceUnavailable, "Serviço externo indisponível"),
            _ => (StatusCodes.Status500InternalServerError, "Erro inesperado"),
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Erro não tratado em {Path}", context.Request.Path);
        else
            logger.LogWarning("{Titulo}: {Mensagem}", titulo, exception.Message);

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = titulo,
                // Em erro inesperado não expomos detalhes internos.
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "Tente novamente em instantes."
                    : exception.Message,
            },
        });
    }
}
