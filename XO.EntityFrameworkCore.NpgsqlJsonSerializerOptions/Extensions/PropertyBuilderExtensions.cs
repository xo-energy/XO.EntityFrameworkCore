using System.Text.Json;
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
    /// <list type="bullet">
    /// <item>Sets the property's value converter to <see cref="JsonSerializerValueConverter{TModel}"/></item>
    /// <item>Sets the property's value comparer to <see cref="JsonSerializerValueComparer{T}"/>; or, when <paramref name="useJsonSerializerValueComparer"/> is <see langword="false"/>, <see langword="null"/></item>
    /// </list>
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property value.</typeparam>
    /// <param name="propertyBuilder">The <see cref="PropertyBuilder{TProperty}"/> to configure.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use when serializing and de-serializing JSON values.</param>
    /// <param name="useJsonSerializerValueComparer">Whether to use <see cref="JsonSerializerValueComparer{T}"/> when tracking changes.</param>
    /// <returns>The same <see cref="PropertyBuilder{TProperty}"/> instance.</returns>
    public static PropertyBuilder<TProperty> UseJsonSerializerOptions<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        JsonSerializerOptions options,
        bool useJsonSerializerValueComparer = true)
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);
        ArgumentNullException.ThrowIfNull(options);

        var converter = new JsonSerializerValueConverter<TProperty>(options);
        var comparer = useJsonSerializerValueComparer ? new JsonSerializerValueComparer<TProperty>(options) : null;

        propertyBuilder.HasConversion(converter, comparer);

        return propertyBuilder;
    }
}
