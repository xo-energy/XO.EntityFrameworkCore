using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

/// <summary>
/// An Entity Framework Core plugin that enables support for <see cref="JsonSerializerOptions"/> in the Npgsql provider.
/// </summary>
public sealed class NpgsqlJsonSerializerOptionsExtension : IDbContextOptionsExtension
{
    private readonly JsonSerializerOptionsProvider _jsonSerializerOptionsProvider;

    private DbContextOptionsExtensionInfo? _info;

    /// <summary>
    /// Initializes a new instance of <see cref="NpgsqlJsonSerializerOptionsExtension"/>.
    /// </summary>
    public NpgsqlJsonSerializerOptionsExtension()
    {
        _jsonSerializerOptionsProvider = new JsonSerializerOptionsProvider();
    }

    /// <inheritdoc/>
    public DbContextOptionsExtensionInfo Info
        => _info ??= new ExtensionInfo(this);

    /// <summary>
    /// Gets or sets the default <see cref="JsonSerializerOptions"/> to use when serializing JSON column values.
    /// </summary>
    public JsonSerializerOptions? DefaultJsonSerializerOptions
    {
        get => _jsonSerializerOptionsProvider.DefaultJsonSerializerOptions;
        set => _jsonSerializerOptionsProvider.DefaultJsonSerializerOptions = value;
    }

    /// <summary>
    /// Gets or sets whether to use <see cref="JsonSerializerValueComparer{T}"/> when tracking changes for objects mapped to JSON columns.
    /// </summary>
    public bool UseJsonSerializerValueComparer
    {
        get => _jsonSerializerOptionsProvider.UseJsonSerializerValueComparer;
        set => _jsonSerializerOptionsProvider.UseJsonSerializerValueComparer = value;
    }

    /// <inheritdoc/>
    public void ApplyServices(IServiceCollection services)
    {
        new EntityFrameworkRelationalServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, JsonSerializerOptionsConventionSetPlugin>()
            .TryAdd<IMemberTranslatorPlugin, NpgsqlJsonMemberTranslatorPlugin>()
            ;

        // not an efcore-defined service, so add it directly to the service collection
        services.TryAddSingleton<IJsonSerializerOptionsProvider>(_jsonSerializerOptionsProvider);
    }

    /// <summary>
    /// This extension does not require any options, so this method does nothing.
    /// </summary>
    public void Validate(IDbContextOptions options)
    {
        // pass
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        private static readonly JsonSerializerOptions _debugOptions = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = false,
        };
        private static readonly JsonSerializerOptions _debugOptionsIndented = new(_debugOptions)
        {
            WriteIndented = true,
        };

        private readonly NpgsqlJsonSerializerOptionsExtension _extension;

        public ExtensionInfo(NpgsqlJsonSerializerOptionsExtension extension)
            : base(extension)
        {
            _extension = extension;
        }

        public override bool IsDatabaseProvider
            => false;

        public override string LogFragment
            => $"{nameof(NpgsqlJsonSerializerOptions)} {nameof(_extension.DefaultJsonSerializerOptions)}={JsonSerializer.Serialize(_extension.DefaultJsonSerializerOptions, _debugOptions)} {nameof(_extension.UseJsonSerializerValueComparer)}={_extension.UseJsonSerializerValueComparer}";

        public override int GetServiceProviderHashCode()
            => HashCode.Combine(_extension.DefaultJsonSerializerOptions, _extension.UseJsonSerializerValueComparer);

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo.Add(
                $"{nameof(NpgsqlJsonSerializerOptions)}:{nameof(_extension.DefaultJsonSerializerOptions)}",
                JsonSerializer.Serialize(_extension.DefaultJsonSerializerOptions, _debugOptionsIndented));
            debugInfo.Add(
                $"{nameof(NpgsqlJsonSerializerOptions)}:{nameof(_extension.UseJsonSerializerValueComparer)}",
                _extension.UseJsonSerializerValueComparer.ToString());
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            if (other.Extension is not NpgsqlJsonSerializerOptionsExtension otherExtension)
                return false;

            if (!Object.Equals(_extension.DefaultJsonSerializerOptions, otherExtension.DefaultJsonSerializerOptions))
                return false;

            if (_extension.UseJsonSerializerValueComparer != otherExtension.UseJsonSerializerValueComparer)
                return false;

            return true;
        }
    }
}
