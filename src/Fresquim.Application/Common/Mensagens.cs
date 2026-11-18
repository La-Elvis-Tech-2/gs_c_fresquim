namespace Fresquim.Application.Common;

/// <summary>
/// Mensagens de validação em português usadas nos atributos dos DTOs.
/// {1} e {2} são os limites do atributo (máximo/mínimo ou mínimo/máximo, conforme o caso).
/// </summary>
public static class Mensagens
{
    public const string Obrigatorio = "Este campo é obrigatório.";
    public const string TamanhoEntre = "Deve ter entre {2} e {1} caracteres.";
    public const string TamanhoMaximo = "Deve ter no máximo {1} caracteres.";
    public const string Faixa = "Deve estar entre {1} e {2}.";
    public const string PaginaMinima = "A página deve ser 1 ou maior.";
}
