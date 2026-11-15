using Fresquim.Domain.Common;
using Fresquim.Domain.Pontos;

namespace Fresquim.Application.Pontos;

/// <summary>Converte entre requisições, entidades e respostas da API.</summary>
public static class PontoMapper
{
    public static PontoResponse ParaResponse(this PontoApoio ponto) => new(
        ponto.Id,
        ponto.Nome,
        ponto.Tipo,
        ponto.Status,
        ponto.Endereco.Logradouro,
        ponto.Endereco.Bairro,
        ponto.Localizacao.Latitude,
        ponto.Localizacao.Longitude,
        ponto.Horario,
        ponto.Dica,
        Detalhes(ponto),
        ponto.CriadoEm,
        ponto.AtualizadoEm);

    /// <summary>Campos específicos de cada tipo (polimorfismo com pattern matching).</summary>
    private static IReadOnlyDictionary<string, object> Detalhes(PontoApoio ponto) => ponto switch
    {
        Bebedouro b => new Dictionary<string, object> { ["aguaGelada"] = b.AguaGelada },
        Sombra s => new Dictionary<string, object> { ["capacidadePessoas"] = s.CapacidadePessoas },
        PostoSaude p => new Dictionary<string, object> { ["atendeEmergencia"] = p.AtendeEmergencia },
        Estacao e => new Dictionary<string, object> { ["linha"] = e.Linha },
        _ => new Dictionary<string, object>(),
    };

    /// <summary>Cria a subclasse certa a partir do tipo informado (Factory Method).</summary>
    public static PontoApoio CriarEntidade(SalvarPontoRequest r)
    {
        var endereco = new Endereco(r.Logradouro, r.Bairro);
        var local = new Coordenada(r.Latitude, r.Longitude);

        return r.Tipo switch
        {
            TipoPonto.Bebedouro => new Bebedouro(r.Nome, endereco, local, r.Horario, r.AguaGelada ?? false),
            TipoPonto.Sombra => new Sombra(r.Nome, endereco, local, r.Horario,
                r.CapacidadePessoas ?? throw new DomainException("Informe a capacidade de pessoas da sombra.")),
            TipoPonto.PostoSaude => new PostoSaude(r.Nome, endereco, local, r.Horario, r.AtendeEmergencia ?? false),
            TipoPonto.Estacao => new Estacao(r.Nome, endereco, local, r.Horario,
                r.Linha ?? throw new DomainException("Informe a linha da estação.")),
            _ => throw new DomainException("Tipo de ponto inválido."),
        };
    }

    /// <summary>Aplica uma edição em um ponto existente (o tipo não pode mudar).</summary>
    public static void AplicarEdicao(PontoApoio ponto, SalvarPontoRequest r)
    {
        if (ponto.Tipo != r.Tipo)
            throw new DomainException("Não é possível alterar o tipo de um ponto. Cadastre um novo.");

        ponto.AtualizarDados(r.Nome, new Endereco(r.Logradouro, r.Bairro), new Coordenada(r.Latitude, r.Longitude), r.Horario);

        switch (ponto)
        {
            case Bebedouro b when r.AguaGelada is { } gelada:
                b.DefinirAguaGelada(gelada);
                break;
            case Sombra s when r.CapacidadePessoas is { } capacidade:
                s.DefinirCapacidade(capacidade);
                break;
            case PostoSaude p when r.AtendeEmergencia is { } emergencia:
                p.DefinirAtendeEmergencia(emergencia);
                break;
            case Estacao e when r.Linha is { } linha:
                e.DefinirLinha(linha);
                break;
        }
    }
}
