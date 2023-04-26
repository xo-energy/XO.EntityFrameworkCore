using System.Text.Json;

namespace XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

/// <summary>
/// Provides the default serializer options to the service collection.
/// </summary>
public interface IJsonSerializerOptionsProvider
{
    /// <summary>
    /// Gets the default <see cref="JsonSerializerOptions"/> to use when serializing and de-serializing values.
    /// </summary>
    JsonSerializerOptions? DefaultJsonSerializerOptions { get; }

    /// <summary>
    /// Gets whether to use <see cref="JsonSerializerValueComparer{T}"/> when tracking changes for objects mapped to JSON columns.
    /// </summary>
    bool UseJsonSerializerValueComparer { get; }
}

/// <summary>
/// Used by <see cref="NpgsqlJsonSerializerOptionsExtension"/> to configure the default serializer options.
/// </summary>
public sealed class JsonSerializerOptionsProvider : IJsonSerializerOptionsProvider
{
    /// <summary>
    /// Initializes a new instance of <see cref="JsonSerializerOptionsProvider"/> with default values.
    /// </summary>
    public JsonSerializerOptionsProvider()
    {
        DefaultJsonSerializerOptions = null;
        UseJsonSerializerValueComparer = true;
    }

    /// <summary>
    /// Gets or sets the default <see cref="JsonSerializerOptions"/> to use when serializing and de-serializing values.
    /// </summary>
    public JsonSerializerOptions? DefaultJsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets whether to use <see cref="JsonSerializerValueComparer{T}"/> when tracking changes for objects mapped to JSON columns.
    /// </summary>
    public bool UseJsonSerializerValueComparer { get; set; }
}
