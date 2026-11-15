using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fresquim.Infrastructure.Persistence;

/// <summary>Garante que toda data lida do banco volte marcada como UTC.</summary>
public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    valor => valor.Kind == DateTimeKind.Utc ? valor : valor.ToUniversalTime(),
    valor => DateTime.SpecifyKind(valor, DateTimeKind.Utc));
