using Fresquim.Domain.Pontos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fresquim.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeia a hierarquia de pontos em uma única tabela (TPH), com a coluna "TipoPonto" como discriminador.
/// Endereço e coordenada são objetos de valor (complex types) gravados como colunas da própria tabela.
/// </summary>
public class PontoApoioConfiguration : IEntityTypeConfiguration<PontoApoio>
{
    public const string ColunaTipo = "TipoPonto";

    public void Configure(EntityTypeBuilder<PontoApoio> builder)
    {
        builder.ToTable("PontosApoio");
        builder.HasKey(p => p.Id);

        builder.HasDiscriminator<TipoPonto>(ColunaTipo)
            .HasValue<Bebedouro>(TipoPonto.Bebedouro)
            .HasValue<Sombra>(TipoPonto.Sombra)
            .HasValue<PostoSaude>(TipoPonto.PostoSaude)
            .HasValue<Estacao>(TipoPonto.Estacao);

        builder.Property(p => p.Nome).HasMaxLength(120).IsRequired();
        builder.Property(p => p.Horario).HasMaxLength(80).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        builder.ComplexProperty(p => p.Endereco, e =>
        {
            e.Property(x => x.Logradouro).HasColumnName("Logradouro").HasMaxLength(150).IsRequired();
            e.Property(x => x.Bairro).HasColumnName("Bairro").HasMaxLength(80).IsRequired();
        });

        builder.ComplexProperty(p => p.Localizacao, c =>
        {
            c.Property(x => x.Latitude).HasColumnName("Latitude");
            c.Property(x => x.Longitude).HasColumnName("Longitude");
        });

        // Propriedades calculadas em código (não viram coluna).
        builder.Ignore(p => p.Tipo);
        builder.Ignore(p => p.Dica);
        builder.Ignore(p => p.EstaDisponivel);

        builder.HasIndex(p => p.Nome);
    }
}

public class BebedouroConfiguration : IEntityTypeConfiguration<Bebedouro>
{
    public void Configure(EntityTypeBuilder<Bebedouro> builder) =>
        builder.Property(b => b.AguaGelada);
}

public class SombraConfiguration : IEntityTypeConfiguration<Sombra>
{
    public void Configure(EntityTypeBuilder<Sombra> builder) =>
        builder.Property(s => s.CapacidadePessoas);
}

public class PostoSaudeConfiguration : IEntityTypeConfiguration<PostoSaude>
{
    public void Configure(EntityTypeBuilder<PostoSaude> builder) =>
        builder.Property(p => p.AtendeEmergencia);
}

public class EstacaoConfiguration : IEntityTypeConfiguration<Estacao>
{
    public void Configure(EntityTypeBuilder<Estacao> builder) =>
        builder.Property(e => e.Linha).HasMaxLength(60);
}
