using Fresquim.Domain.Clima;
using Fresquim.Domain.Pontos;

namespace Fresquim.Application.Abstractions;

/// <summary>Fonte externa de dados do tempo (implementada com a API Open-Meteo).</summary>
public interface IClimaProvider
{
    Task<CondicaoClimatica> ObterCondicaoAtualAsync(Coordenada local, CancellationToken ct = default);
}
