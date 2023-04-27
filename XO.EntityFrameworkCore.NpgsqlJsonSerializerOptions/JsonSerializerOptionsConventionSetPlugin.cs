using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

/// <summary>
/// Customizes the Entity Framework Core convention set by adding <see cref="JsonSerializerOptionsConvention"/>.
/// </summary>
public sealed class JsonSerializerOptionsConventionSetPlugin : IConventionSetPlugin
{
    private readonly IJsonSerializerOptionsProvider _jsonSerializerOptionsProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="JsonSerializerOptionsConventionSetPlugin"/>.
    /// </summary>
    /// <param name="jsonSerializerOptionsProvider">Provides the default serializer options.</param>
    public JsonSerializerOptionsConventionSetPlugin(IJsonSerializerOptionsProvider jsonSerializerOptionsProvider)
    {
        _jsonSerializerOptionsProvider = jsonSerializerOptionsProvider;
    }

    /// <inheritdoc/>
    public ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        var convention = new JsonSerializerOptionsConvention(_jsonSerializerOptionsProvider);

        conventionSet.ModelFinalizingConventions.Add(convention);

        return conventionSet;
    }
}
