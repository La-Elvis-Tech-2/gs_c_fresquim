using Fresquim.Domain.Pontos;

namespace Fresquim.Tests.Helpers;

/// <summary>Cria pontos de exemplo para os testes.</summary>
internal static class PontosFake
{
    public static readonly Coordenada Paulista = new(-23.5614, -46.6559);

    public static Bebedouro Bebedouro(string nome = "Bebedouro Trianon", double lat = -23.5614, double lon = -46.6559, bool gelada = false) =>
        new(nome, new Endereco("Rua Peixoto Gomide, 949", "Bela Vista"), new Coordenada(lat, lon), "6h às 18h", gelada);

    public static Sombra Sombra(int capacidade = 30, double lat = -23.5717, double lon = -46.6448) =>
        new("Praça Oswaldo Cruz", new Endereco("Praça Oswaldo Cruz, s/n", "Paraíso"), new Coordenada(lat, lon), "24 horas", capacidade);

    public static PostoSaude Posto(bool emergencia = true) =>
        new("UBS Humaitá", new Endereco("Rua Humaitá, 520", "Bela Vista"), new Coordenada(-23.5636, -46.6448), "7h às 19h", emergencia);

    public static Estacao Estacao(string bairro = "Consolação") =>
        new("Estação Consolação", new Endereco("Avenida Paulista, 2163", bairro), new Coordenada(-23.5577, -46.6608), "4h40 à 0h", "Linha 2-Verde");
}
