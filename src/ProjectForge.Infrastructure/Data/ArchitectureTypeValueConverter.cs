using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjectForge.Core.Enums;

namespace ProjectForge.Infrastructure.Data;

public sealed class ArchitectureTypeValueConverter : ValueConverter<ArchitectureType, string>
{
    public static readonly ArchitectureTypeValueConverter Instance = new();

    private ArchitectureTypeValueConverter()
        : base(
            arch => arch.ToString(),
            value => Parse(value))
    {
    }

    private static ArchitectureType Parse(string value)
    {
        if (Enum.TryParse<ArchitectureType>(value, ignoreCase: true, out var arch))
            return arch;

        return value.Equals("Laravel", StringComparison.OrdinalIgnoreCase)
            ? ArchitectureType.Php
            : ArchitectureType.DotNet;
    }
}
