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

    /// <summary>
    /// Initializes a new instance of <see cref="NpgsqlJsonSerializerOptionsExtension"/>.
    /// </summary>
    public NpgsqlJsonSerializerOptionsExtension()
    {
        Info = new ExtensionInfo(this);
        _jsonSerializerOptionsProvider = new JsonSerializerOptionsProvider();
    }

    /// <inheritdoc/>
    public DbContextOptionsExtensionInfo Info { get; }

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
        services.Replace(
            ServiceDescriptor.Singleton<IJsonSerializerOptionsProvider>(_jsonSerializerOptionsProvider));
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
        private readonly NpgsqlJsonSerializerOptionsExtension _extension;

        public ExtensionInfo(NpgsqlJsonSerializerOptionsExtension extension)
            : base(extension)
        {
            _extension = extension;
        }

        public override bool IsDatabaseProvider
            => false;

        public override string LogFragment
            => nameof(NpgsqlJsonSerializerOptions);

        public override int GetServiceProviderHashCode()
            => default;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            // could include individual option values here and in 'LogFragment' if you wanted to be thorough
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => true;
    }
}