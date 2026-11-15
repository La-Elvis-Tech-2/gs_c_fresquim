using Fresquim.Domain.Avisos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fresquim.Infrastructure.Persistence.Configurations;

public class AvisoConfiguration : IEntityTypeConfiguration<Aviso>
{
    public void Configure(EntityTypeBuilder<Aviso> builder)
    {
        builder.ToTable("Avisos");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Descricao).HasMaxLength(280).IsRequired();
        builder.Property(a => a.NomeLocalSugerido).HasMaxLength(120);
        builder.Property(a => a.RespostaPrefeitura).HasMaxLength(280);

        // Se o ponto for removido, os avisos ficam no histórico sem o vínculo.
        builder.HasOne(a => a.PontoApoio)
            .WithMany()
            .HasForeignKey(a => a.PontoApoioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(a => a.EstaFinalizado);

        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.PontoApoioId);
    }
}
