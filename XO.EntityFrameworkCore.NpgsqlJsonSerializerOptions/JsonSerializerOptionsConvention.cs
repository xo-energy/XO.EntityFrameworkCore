using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

/// <summary>
/// Configures entity properties to use the default <see cref="JsonSerializerOptions"/>.
/// </summary>
public sealed class JsonSerializerOptionsConvention : IModelFinalizingConvention
{
    private readonly MethodInfo _configureMethodInfo;
    private readonly IJsonSerializerOptionsProvider _jsonSerializerOptionsProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="JsonSerializerOptionsConvention"/>.
    /// </summary>
    /// <param name="jsonSerializerOptionsProvider">Provides the configured serializer options.</param>
    public JsonSerializerOptionsConvention(IJsonSerializerOptionsProvider jsonSerializerOptionsProvider)
    {
        _configureMethodInfo = new Action<IConventionProperty>(Configure<object>).Method.GetGenericMethodDefinition();
        _jsonSerializerOptionsProvider = jsonSerializerOptionsProvider;
    }

    /// <inheritdoc/>
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        if (_jsonSerializerOptionsProvider.DefaultJsonSerializerOptions == null)
            return;

        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var columnType = property.GetColumnType();
                if (columnType != "json" && columnType != "jsonb")
                    continue;

                _configureMethodInfo.MakeGenericMethod(property.ClrType)
                    .CreateDelegate<Action<IConventionProperty>>(this)
                    .Invoke(property);
            }
        }
    }

    private void Configure<TProperty>(IConventionProperty property)
    {
        // the calling method checked for null
        var options = _jsonSerializerOptionsProvider.DefaultJsonSerializerOptions!;

        var converter = new JsonSerializerValueConverter<TProperty>(options);
        var comparer = _jsonSerializerOptionsProvider.UseJsonSerializerValueComparer ? new JsonSerializerValueComparer<TProperty>(options) : null;

        var canSetConversion = property.Builder.CanSetConversion(converter);
        if (canSetConversion)
        {
            property.SetValueConverter(converter);
        }

        if (canSetConversion && property.Builder.CanSetValueComparer(comparer))
        {
            property.SetValueComparer(comparer);
        }
    }
}
