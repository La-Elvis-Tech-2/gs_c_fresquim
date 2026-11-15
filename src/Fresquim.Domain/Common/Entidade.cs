namespace Fresquim.Domain.Common;

/// <summary>Base de todas as entidades: identidade e datas de criação/atualização.</summary>
public abstract class Entidade
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CriadoEm { get; protected set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; protected set; }

    protected void MarcarAtualizacao() => AtualizadoEm = DateTime.UtcNow;
}
