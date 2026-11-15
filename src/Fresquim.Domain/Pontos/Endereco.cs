using Fresquim.Domain.Common;

namespace Fresquim.Domain.Pontos;

/// <summary>Objeto de valor com o endereço do ponto.</summary>
public sealed record Endereco
{
    public string Logradouro { get; }
    public string Bairro { get; }

    public Endereco(string logradouro, string bairro)
    {
        Logradouro = Guard.Texto(logradouro, "Logradouro", 150);
        Bairro = Guard.Texto(bairro, "Bairro", 80);
    }

    public override string ToString() => $"{Logradouro} - {Bairro}";
}
