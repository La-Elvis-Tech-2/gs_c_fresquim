namespace Fresquim.Domain.Common;

/// <summary>Regra de negócio violada (a API devolve HTTP 400).</summary>
public class DomainException(string message) : Exception(message);
