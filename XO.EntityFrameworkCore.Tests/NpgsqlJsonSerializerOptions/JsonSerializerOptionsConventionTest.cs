using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

public class JsonSerializerOptionsConventionTest
{
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new();

    [Fact]
    public void ConfiguresValueComparer_WithDefaultOptions()
    {
        AssertComparer(nameof(TestModel.Data), DefaultJsonSerializerOptions);
    }

    [Fact]
    public void ConfiguresValueComparer_WithExplicitOptions()
    {
        AssertComparer(nameof(TestModel.DataExplicit));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConfiguresValueConverter_WithDefaultOptions(bool useJsonSerializerValueComparer)
    {
        AssertConverter(nameof(TestModel.Data), DefaultJsonSerializerOptions, useJsonSerializerValueComparer);
    }

    [Fact]
    public void ConfiguresValueConverter_WithExplicitOptions()
    {
        AssertConverter(nameof(TestModel.DataExplicit));
    }

    [Fact]
    public void DoesNotConfigureValueComparer_WithDefaultOptions_WhenUseJsonSerializerValueComparerIsFalse()
    {
        AssertNotComparer(nameof(TestModel.Data), DefaultJsonSerializerOptions, useJsonSerializerValueComparer: false);
    }

    [Fact]
    public void DoesNotConfigureValueConverter_WithoutDefaultOptions()
    {
        AssertNotConverter(nameof(TestModel.Data));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DoesNotConfigureValueComparer_WithoutDefaultOptions(bool useJsonSerializerValueComparer)
    {
        AssertNotComparer(nameof(TestModel.Data), useJsonSerializerValueComparer: useJsonSerializerValueComparer);
    }

    [Fact]
    public void DoesNotOverrideValueComparer_WithCustomValueComparer()
    {
        AssertNotComparer(nameof(TestModel.DataCustomValueComparer), DefaultJsonSerializerOptions);
    }

    [Fact]
    public void DoesNotOverrideValueComparer_WithCustomValueConverter()
    {
        AssertNotComparer(nameof(TestModel.DataCustomValueConverter), DefaultJsonSerializerOptions);
    }

    [Fact]
    public void DoesNotOverrideValueConverter_WithCustomValueConverter()
    {
        AssertNotConverter(nameof(TestModel.DataCustomValueConverter), DefaultJsonSerializerOptions);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DoesNotOverrideValueComparer_WithDefaultAndExplicitOptions_WhenExplicitUseValueComparerIsFalse(bool defaultUseValueComparer)
    {
        AssertNotComparer(nameof(TestModel.DataExplicitNoValueComparer), DefaultJsonSerializerOptions, defaultUseValueComparer);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DoesNotOverrideValueComparer_WithDefaultAndExplicitOptions_WhenExplicitUseValueComparerIsTrue(bool defaultUseValueComparer)
    {
        AssertComparer(nameof(TestModel.DataExplicit), DefaultJsonSerializerOptions, defaultUseValueComparer);
    }

    [Fact]
    public void DoesNotOverrideValueConverter_WithDefaultAndExplicitOptions()
    {
        AssertHelper(
            nameof(TestModel.DataExplicit),
            property =>
            {
                var converter = Assert.IsType<JsonSerializerValueConverter<TestJsonDataObject>>(
                    property!.GetValueConverter());

                Assert.Equal("age", converter.ConvertName(nameof(TestJsonDataObject.Age)));
            },
            DefaultJsonSerializerOptions,
            true);
    }

    private static void AssertComparer(
        string propertyName,
        JsonSerializerOptions? options = null,
        bool useJsonSerializerValueComparer = true)
        => AssertHelper(
            propertyName,
            property => Assert.IsType<JsonSerializerValueComparer<TestJsonDataObject>>(property.GetValueComparer()),
            options,
            useJsonSerializerValueComparer);

    private static void AssertConverter(
        string propertyName,
        JsonSerializerOptions? options = null,
        bool useJsonSerializerValueComparer = true)
        => AssertHelper(
            propertyName,
            property => Assert.IsType<JsonSerializerValueConverter<TestJsonDataObject>>(property.GetValueConverter()),
            options,
            useJsonSerializerValueComparer);

    private static void AssertNotComparer(
        string propertyName,
        JsonSerializerOptions? options = null,
        bool useJsonSerializerValueComparer = true)
        => AssertHelper(
            propertyName,
            property => Assert.IsNotType<JsonSerializerValueComparer<TestJsonDataObject>>(property.GetValueComparer()),
            options,
            useJsonSerializerValueComparer);

    private static void AssertNotConverter(
        string propertyName,
        JsonSerializerOptions? options = null,
        bool useJsonSerializerValueComparer = true)
        => AssertHelper(
            propertyName,
            property => Assert.IsNotType<JsonSerializerValueConverter<TestJsonDataObject>>(property.GetValueConverter()),
            options,
            useJsonSerializerValueComparer);

    private static void AssertHelper(
        string propertyName,
        Action<IProperty> assert,
        JsonSerializerOptions? options,
        bool useJsonSerializerValueComparer)
    {
        using var context = new TestContext(
            configureOptions: (optionsBuilder) =>
            {
                optionsBuilder.UseNpgsqlJsonSerializerOptions(
                    options,
                    useJsonSerializerValueComparer);
            });

        var property = context.Model
            .FindEntityType(typeof(TestModel))?
            .FindProperty(propertyName);

        Assert.NotNull(property);

        assert(property);
    }
}