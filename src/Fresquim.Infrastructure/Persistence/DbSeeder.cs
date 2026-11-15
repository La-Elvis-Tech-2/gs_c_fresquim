using Fresquim.Domain.Pontos;
using Microsoft.EntityFrameworkCore;

namespace Fresquim.Infrastructure.Persistence;

/// <summary>
/// Cadastra os pontos iniciais da região da Av. Paulista (os mesmos do app Fresquim).
/// Endereços de locais públicos reais; horários, status e detalhes são simulados.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(FresquimDbContext db, CancellationToken ct = default)
    {
        if (await db.Pontos.AnyAsync(ct)) return;

        var pontos = new PontoApoio[]
        {
            new Bebedouro("Bebedouro do Parque Trianon", new("Rua Peixoto Gomide, 949", "Bela Vista"),
                new(-23.5614, -46.6559), "Todos os dias, 6h às 18h", aguaGelada: true),
            new Sombra("Praça Oswaldo Cruz (sombra)", new("Praça Oswaldo Cruz, s/n", "Paraíso"),
                new(-23.5717, -46.6448), "Aberto 24 horas", capacidadePessoas: 30),
            new PostoSaude("UBS Bela Vista", new("Rua Frei Caneca, 1100", "Bela Vista"),
                new(-23.5562, -46.6544), "Segunda a sexta, 7h às 19h", atendeEmergencia: false),
            new Estacao("Estação Consolação do Metrô", new("Avenida Paulista, 2163", "Consolação"),
                new(-23.5577, -46.6608), "Todos os dias, 4h40 à meia-noite", linha: "Linha 2-Verde"),
            new Bebedouro("Bebedouro da Praça do Ciclista", new("Avenida Paulista, 2439", "Consolação"),
                new(-23.5553, -46.6626), "Aberto 24 horas", aguaGelada: false),
            new Sombra("Parque da Aclimação (sombra)", new("Rua Muniz de Sousa, 1119", "Aclimação"),
                new(-23.5714, -46.6299), "Todos os dias, 5h às 20h", capacidadePessoas: 200),
            new Estacao("Estação Brigadeiro do Metrô", new("Avenida Paulista, 33", "Bela Vista"),
                new(-23.5686, -46.6496), "Todos os dias, 4h40 à meia-noite", linha: "Linha 2-Verde"),
            new PostoSaude("UBS Humaitá", new("Rua Humaitá, 520", "Bela Vista"),
                new(-23.5636, -46.6448), "Segunda a sexta, 7h às 19h", atendeEmergencia: true),
        };

        pontos[4].ReportarProblema(); // Bebedouro da Praça do Ciclista começa com problema.

        await db.Pontos.AddRangeAsync(pontos, ct);
        await db.SaveChangesAsync(ct);
    }
}
