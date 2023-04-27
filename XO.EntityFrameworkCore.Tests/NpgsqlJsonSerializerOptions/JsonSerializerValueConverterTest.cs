using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

public sealed class JsonSerializerValueConverterTest
{
    private JsonSerializerValueConverter<TestJsonDataObject> CreateValueConverter(JsonSerializerOptions? options = default)
        => new(options ?? new JsonSerializerOptions());

    [Fact]
    public void RespectsJsonPropertyNameAttribute()
    {
        var valueConverter = CreateValueConverter(
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var value = CreateSampleValue();
        var attribute = typeof(TestJsonDataObject)
            .GetProperty(nameof(TestJsonDataObject.Attributes))!
            .GetCustomAttribute<JsonPropertyNameAttribute>()!;

        var converted = (string)valueConverter.ConvertToProvider(value)!;

        using var document = JsonDocument.Parse(converted);

        Assert.True(document.RootElement.TryGetProperty(attribute.Name, out _));
    }

    [Fact]
    public void ReturnsNull_WhenValueIsNull()
    {
        var valueConverter = CreateValueConverter();
        var value = CreateSampleValue();

        Assert.Multiple(
            () => Assert.Null(valueConverter.ConvertToProvider(null)),
            () => Assert.Null(valueConverter.ConvertFromProvider(null)));
    }

    [Fact]
    public void RoundTrippedValueIsEqual()
    {
        var valueConverter = CreateValueConverter();
        var value = CreateSampleValue();

        var converted = valueConverter.ConvertToProvider(value);
        var convertedBack = valueConverter.ConvertFromProvider(converted);

        Assert.Equal(value, convertedBack);
    }

    private static TestJsonDataObject CreateSampleValue()
        => new TestJsonDataObject("John Doe", 42);
}
