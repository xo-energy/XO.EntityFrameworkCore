using System.Text.Json;
using System.Text.Json.Serialization;

namespace XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

public class NpgsqlJsonSerializerOptionsExtensionTest
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.General)
    {
        Converters = {
            new JsonStringEnumConverter(),
        },
    };

    [Fact]
    public void LogFragment_ContainsAllowTrailingCommas()
    {
        var extension = new NpgsqlJsonSerializerOptionsExtension()
        {
            DefaultJsonSerializerOptions = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
            },
        };

        var fragment = extension.Info.LogFragment;

        Assert.Contains("\"AllowTrailingCommas\":true", fragment);
    }

    [Fact]
    public void LogFragment_ContainsUseJsonSerializerValueComparer()
    {
        var extension = new NpgsqlJsonSerializerOptionsExtension();

        var fragment = extension.Info.LogFragment;

        Assert.Contains("UseJsonSerializerValueComparer=True", fragment);
    }

    [Fact]
    public void PopulateDebugInfo_AddsDefaultJsonSerializerOptions()
    {
        var extension = new NpgsqlJsonSerializerOptionsExtension()
        {
            DefaultJsonSerializerOptions = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
            },
        };
        var debugInfo = new Dictionary<string, string>();

        extension.Info.PopulateDebugInfo(debugInfo);

        Assert.True(debugInfo.ContainsKey("NpgsqlJsonSerializerOptions:DefaultJsonSerializerOptions"));
        var serializedOptions = debugInfo["NpgsqlJsonSerializerOptions:DefaultJsonSerializerOptions"];
        var deserializedOptions = JsonSerializer.Deserialize<JsonSerializerOptions>(serializedOptions, _options);
        Assert.NotNull(deserializedOptions);
        Assert.True(deserializedOptions.AllowTrailingCommas);
    }

    [Fact]
    public void PopulateDebugInfo_AddsDefaultJsonSerializerOptions_WithConverters()
    {
        var extension = new NpgsqlJsonSerializerOptionsExtension()
        {
            DefaultJsonSerializerOptions = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter() },
            },
        };
        var debugInfo = new Dictionary<string, string>();

        extension.Info.PopulateDebugInfo(debugInfo);

        Assert.True(debugInfo.ContainsKey("NpgsqlJsonSerializerOptions:DefaultJsonSerializerOptions"));
        var serializedOptions = debugInfo["NpgsqlJsonSerializerOptions:DefaultJsonSerializerOptions"];
        var deserializedOptions = JsonSerializer.Deserialize<JsonSerializerOptions>(serializedOptions, _options);
        Assert.NotNull(deserializedOptions);
    }

    [Fact]
    public void PopulateDebugInfo_AddsDefaultJsonSerializerOptions_WithWebDefaults()
    {
        var extension = new NpgsqlJsonSerializerOptionsExtension()
        {
            DefaultJsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web),
        };
        var debugInfo = new Dictionary<string, string>();

        extension.Info.PopulateDebugInfo(debugInfo);

        Assert.True(debugInfo.ContainsKey("NpgsqlJsonSerializerOptions:DefaultJsonSerializerOptions"));
        var serializedOptions = debugInfo["NpgsqlJsonSerializerOptions:DefaultJsonSerializerOptions"];
        var deserializedOptions = JsonSerializer.Deserialize<JsonSerializerOptions>(serializedOptions, _options);
        Assert.NotNull(deserializedOptions);
    }

    [Fact]
    public void PopulateDebugInfo_AddsUseJsonSerializerValueComparer()
    {
        var extension = new NpgsqlJsonSerializerOptionsExtension();
        var debugInfo = new Dictionary<string, string>();

        extension.Info.PopulateDebugInfo(debugInfo);

        Assert.True(debugInfo.ContainsKey("NpgsqlJsonSerializerOptions:UseJsonSerializerValueComparer"));
        var valueComparer = debugInfo["NpgsqlJsonSerializerOptions:UseJsonSerializerValueComparer"];
        Assert.Equal(extension.UseJsonSerializerValueComparer.ToString(), valueComparer);
    }
}
