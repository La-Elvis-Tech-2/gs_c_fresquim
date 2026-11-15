namespace Fresquim.Domain.Common;

/// <summary>Validações reutilizadas pelas entidades.</summary>
internal static class Guard
{
    public static string Texto(string? valor, string campo, int max)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException($"{campo} é obrigatório.");

        var limpo = valor.Trim();
        if (limpo.Length > max)
            throw new DomainException($"{campo} deve ter no máximo {max} caracteres.");

        return limpo;
    }
}
