using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// 
/// </summary>
public static class PropertyBuilderExtensions
{
    /// <summary>
    /// Configures the property to use the specified <see cref="JsonSerializerOptions"/> when serializing and de-serializing JSON values.
    /// </summary>
    /// <remarks>
    /// When <paramref name="options"/> is non-null, this method sets the property's value converter to <see
    /// cref="JsonSerializerValueConverter{TModel}"/> and, optionally, its comparer to <see
    /// cref="JsonSerializerValueComparer{T}"/>.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property value.</typeparam>
    /// <param name="propertyBuilder">The <see cref="PropertyBuilder{TProperty}"/> to configure.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use when serializing and de-serializing JSON values. Pass <see langword="null"/> to ignore the default options.</param>
    /// <param name="useJsonSerializerValueComparer">Whether to use <see cref="JsonSerializerValueComparer{T}"/> when tracking changes.</param>
    /// <returns>The same <see cref="PropertyBuilder{TProperty}"/> instance.</returns>
    public static PropertyBuilder<TProperty> UseJsonSerializerOptions<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        JsonSerializerOptions? options,
        bool useJsonSerializerValueComparer = true)
    {
        propertyBuilder
            .GetInfrastructure()
            .SetJsonSerializerValueConverter<TProperty>(options, useJsonSerializerValueComparer);

        return propertyBuilder;
    }

    internal static void SetJsonSerializerValueConverter<TProperty>(
        this IConventionPropertyBuilder propertyBuilder,
        JsonSerializerOptions? options,
        bool useJsonSerializerValueComparer = true)
    {
        JsonSerializerValueConverter<TProperty>? converter = null;
        JsonSerializerValueComparer<TProperty>? comparer = null;

        if (options != null)
        {
            converter = new JsonSerializerValueConverter<TProperty>(options);
            comparer = useJsonSerializerValueComparer ? new JsonSerializerValueComparer<TProperty>(options) : null;
        }

        if (propertyBuilder.CanSetConversion(converter))
        {
            propertyBuilder.Metadata.SetValueConverter(converter);
        }

        if (propertyBuilder.CanSetValueComparer(comparer))
        {
            propertyBuilder.Metadata.SetValueComparer(comparer);
        }
    }
}
