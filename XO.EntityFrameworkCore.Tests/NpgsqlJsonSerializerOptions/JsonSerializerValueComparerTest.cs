using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

public sealed class JsonSerializerValueComparerTest
{
    private JsonSerializerValueComparer<TestJsonDataObject> CreateValueComparer(JsonSerializerOptions? options = default)
        => new(options ?? new JsonSerializerOptions());

    [Fact]
    public void ReturnsFalse_WhenOneValueIsNull()
    {
        var valueComparer = CreateValueComparer();
        var value = CreateSampleValue();

        Assert.Multiple(
            () => Assert.False(valueComparer.Equals(null, value)),
            () => Assert.False(valueComparer.Equals(value, null)));
    }

    [Fact]
    public void ReturnsFalse_WhenValuesAreNotEqual()
    {
        var valueComparer = CreateValueComparer();
        var value1 = CreateSampleValue();
        var value2 = CreateSampleValue() with
        {
            Name = "Jane Doe",
        };


        Assert.False(valueComparer.Equals(value1, value2));
    }

    [Fact]
    public void ReturnsFalse_WhenValuesAreNotEqual_WithDefaultIgnoreCondition()
    {
        var valueComparer = CreateValueComparer(
            new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            });
        var value1 = CreateSampleValue();
        var value2 = CreateSampleValue() with
        {
            Description = "unidentified",
        };

        Assert.False(valueComparer.Equals(value1, value2));
    }

    [Fact]
    public void ReturnsTrue_WhenBothValuesAreNull()
    {
        var valueComparer = CreateValueComparer();

        Assert.True(valueComparer.Equals(null, null));
    }

    [Fact]
    public void ReturnsTrue_WhenValuesAreEqual()
    {
        var valueComparer = CreateValueComparer();
        var value1 = CreateSampleValue();
        var value2 = CreateSampleValue();

        Assert.True(valueComparer.Equals(value1, value2));
    }

    [Fact]
    public void ReturnsTrue_WhenValuesAreEqual_WithCamelCase()
    {
        var valueComparer = CreateValueComparer(
            new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
        var value1 = CreateSampleValue();
        var value2 = CreateSampleValue();

        Assert.True(valueComparer.Equals(value1, value2));
    }

    [Fact]
    public void ReturnsTrue_WhenValuesAreEqual_WithDefaultIgnoreCondition()
    {
        var valueComparer = CreateValueComparer(
            new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            });
        var value1 = CreateSampleValue();
        var value2 = CreateSampleValue();

        Assert.True(valueComparer.Equals(value1, value2));
    }

    private static TestJsonDataObject CreateSampleValue()
        => new TestJsonDataObject(
            "John Doe",
            42,
            Dependants: 3,
            Attributes: new(Occupation: "foo", Address: null));
}
