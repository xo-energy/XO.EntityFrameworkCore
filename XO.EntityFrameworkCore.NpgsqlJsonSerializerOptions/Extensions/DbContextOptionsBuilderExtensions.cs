using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Declares extension methods to support the <c>NpgsqlJsonSerializerOptions</c> plugin.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Adds support for <see cref="JsonSerializerOptions"/> to the Npgsql provider.
    /// </summary>
    /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder"/> to configure.</param>
    /// <param name="defaultJsonSerializerOptions">The default <see cref="JsonSerializerOptions"/> to use when serializing JSON column values.</param>
    /// <param name="useJsonSerializerValueComparer">Whether to use <see cref="JsonSerializerValueComparer{T}"/> when tracking changes for objects mapped to JSON columns.</param>
    /// <returns>The same <see cref="DbContextOptionsBuilder"/> instance.</returns>
    public static DbContextOptionsBuilder UseNpgsqlJsonSerializerOptions(
        this DbContextOptionsBuilder optionsBuilder,
        JsonSerializerOptions? defaultJsonSerializerOptions = null,
        bool useJsonSerializerValueComparer = true)
    {
        var infrastructure = (IDbContextOptionsBuilderInfrastructure)optionsBuilder;
        var extension = optionsBuilder.Options.FindExtension<NpgsqlJsonSerializerOptionsExtension>()
            ?? new NpgsqlJsonSerializerOptionsExtension();

        extension.DefaultJsonSerializerOptions = defaultJsonSerializerOptions;
        extension.UseJsonSerializerValueComparer = useJsonSerializerValueComparer;

        infrastructure.AddOrUpdateExtension(extension);
        return optionsBuilder;
    }
}
