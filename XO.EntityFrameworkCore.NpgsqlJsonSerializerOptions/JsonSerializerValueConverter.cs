using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

/// <summary>
/// Represents an object that can convert CLR property names to JSON property names.
/// </summary>
/// <remarks>
/// Used by <see cref="NpgsqlJsonMemberTranslator"/> to translate member-access query expressions.
/// </remarks>
public interface IJsonSerializerConvertName
{
    /// <summary>
    /// Converts a CLR property name to a JSON property name.
    /// </summary>
    /// <param name="name">The property name to convert.</param>
    /// <returns>The JSON property name, or <see langword="null"/> if no <see cref="JsonNamingPolicy"/> was configured.</returns>
    string? ConvertName(string name);
}

/// <summary>
/// Converts property values to JSON strings using <see cref="JsonSerializer"/>.
/// </summary>
/// <typeparam name="TModel">The value type to convert.</typeparam>
public sealed class JsonSerializerValueConverter<TModel> : ValueConverter<TModel, string>, IJsonSerializerConvertName
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="JsonSerializerValueConverter{TModel}"/>.
    /// </summary>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use when serializing and deserializing values.</param>
    public JsonSerializerValueConverter(JsonSerializerOptions options) : base(
        model => JsonSerializer.Serialize(model, options),
        value => JsonSerializer.Deserialize<TModel>(value, options)!)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public string? ConvertName(string name)
        => _options.PropertyNamingPolicy?.ConvertName(name);
}
