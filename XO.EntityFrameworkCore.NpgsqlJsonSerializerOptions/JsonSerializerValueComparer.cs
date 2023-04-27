using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

/// <summary>
/// Implements value equality comparison by serializing the values to JSON and comparing the resulting strings.
/// </summary>
/// <typeparam name="T">The type to compare.</typeparam>
public sealed class JsonSerializerValueComparer<T> : ValueComparer<T>
{
    /// <summary>
    /// Initializes a new instance of <see cref="JsonSerializerValueComparer{T}"/>.
    /// </summary>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use when serializing values.</param>
    public JsonSerializerValueComparer(JsonSerializerOptions options) : base(
        (a, b) => JsonSerializer.Serialize(a, options) == JsonSerializer.Serialize(b, options),
        (a) => JsonSerializer.Serialize(a, options).GetHashCode())
    {
        // pass
    }
}
