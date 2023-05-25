using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

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

    [Fact]
    public void ShouldUseSameServiceProvider_ReturnsTrue()
    {
        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        // uses a shared instance of JsonSerializerOptions
        void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsqlJsonSerializerOptions(jsonSerializerOptions);
        }

        using var context1 = new TestContext(configureOptions: Configure);
        using var context2 = new TestContext(configureOptions: Configure);

        AssertServiceProvider(context1, context2, same: true);
    }

    [Fact]
    public void ShouldUseSameServiceProvider_ReturnsFalse()
    {
        // a new instance of DbContextOptionsBuilder is created for each context!
        static void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsqlJsonSerializerOptions(
                new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });
        }

        using var context1 = new TestContext(configureOptions: Configure);
        using var context2 = new TestContext(configureOptions: Configure);

        AssertServiceProvider(context1, context2, same: false);
    }

    private static void AssertServiceProvider(DbContext context1, DbContext context2, bool same)
    {
        var scope1 = context1.GetService<IServiceProvider>();
        var scope2 = context2.GetService<IServiceProvider>();

        var property = scope1.GetType().GetProperty("RootProvider", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var sp1 = property.GetValue(scope1);
        var sp2 = property.GetValue(scope2);

        if (same)
        {
            Assert.Same(sp2, sp1);
        }
        else
        {
            Assert.NotSame(sp2, sp1);
        }
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
