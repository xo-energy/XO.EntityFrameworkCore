using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NSubstitute;

namespace XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

public sealed class NpgsqlJsonMemberTranslatorTest
{
    [Fact]
    public void CanTraverseAttributes()
    {
        using var context = new TestContext(
            configureOptions: builder =>
            {
                builder.UseNpgsqlJsonSerializerOptions(
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));
            });

        var record = new TestModel()
        {
            Data = new TestJsonDataObject("John Doe", 42, Attributes: new(Occupation: "detective")),
        };

        context.TestModels.Add(record);
        context.SaveChanges();

        var occupation = context.TestModels
            .Where(x => x.Id == record.Id)
            .Select(x => x.Data!.Attributes!.Occupation)
            .Single();

        Assert.Equal(record.Data.Attributes!.Occupation, occupation);
    }

    [Fact]
    public void CanTraverseListCount()
    {
        using var context = new TestContext(
            configureOptions: builder =>
            {
                builder.UseNpgsqlJsonSerializerOptions(
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));
            });

        var record = new TestModel()
        {
            Data = new TestJsonDataObject("John Doe", 42, Aliases: new() { "JD", "Johnny" }),
        };

        context.TestModels.Add(record);
        context.SaveChanges();

        var count = context.TestModels
            .Where(x => x.Id == record.Id)
            .Select(x => x.Data!.Aliases!.Count)
            .Single();

        Assert.Equal(record.Data.Aliases!.Count, count);
    }

    [Fact]
    public void DoesNotThrow_WhenProviderIsNotNpgsql()
    {
        using var context = new DbContext(
            new DbContextOptionsBuilder()
                .UseSqlite("Data Source=:memory:")
                .UseNpgsqlJsonSerializerOptions()
                .Options);

        var plugin = context.GetService<IEnumerable<IMemberTranslatorPlugin>>()
            .OfType<NpgsqlJsonMemberTranslatorPlugin>()
            .Single();

        Assert.Empty(plugin.Translators);
    }

    [Fact]
    public void RespectsCamelCaseNamingPolicy()
    {
        using var context = new TestContext(
            configureOptions: builder =>
            {
                builder.UseNpgsqlJsonSerializerOptions(
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));
            });

        var record = new TestModel()
        {
            Data = new TestJsonDataObject("John Doe", 42),
        };

        context.TestModels.Add(record);
        context.SaveChanges();

        var age = context.TestModels
            .Where(x => x.Id == record.Id)
            .Select(x => x.Data!.Age)
            .Single();

        Assert.Equal(record.Data.Age, age);
    }

    [Fact]
    public void ReturnsNull_WhenConverterIsNotConvertName()
    {
        using var context = new TestContext(
            configureOptions: builder =>
            {
                builder.UseNpgsqlJsonSerializerOptions();
            });

        var plugin = context.GetService<IEnumerable<IMemberTranslatorPlugin>>()
            .OfType<NpgsqlJsonMemberTranslatorPlugin>()
            .Single();
        var translator = plugin.Translators
            .OfType<NpgsqlJsonMemberTranslator>()
            .Single();
        var typeMapping = new NpgsqlJsonTypeMapping("jsonb", typeof(TestModel));
        var columnExpression = Substitute.For<ColumnExpression>(
            typeof(TestModel),
            typeMapping);

        columnExpression.TypeMapping.Returns(typeMapping);

        var result = translator.Translate(
            columnExpression,
            typeof(TestModel).GetProperty(nameof(TestModel.Data))!,
            typeof(TestModel),
            null!);

        Assert.Null(result);
    }

    [Fact]
    public void ReturnsNull_WhenConverterReturnsNull()
    {
        using var context = new TestContext(
            configureOptions: builder =>
            {
                builder.UseNpgsqlJsonSerializerOptions();
            });

        var translator = context.GetTranslator();
        var typeMapping = new NpgsqlJsonTypeMapping("jsonb", typeof(TestModel));
        var columnExpression = Substitute.For<ColumnExpression>(
            typeof(TestModel),
            typeMapping);

        typeMapping = (NpgsqlJsonTypeMapping)typeMapping.WithComposedConverter(new NullConvertName());
        columnExpression.TypeMapping.Returns(typeMapping);

        var result = translator.Translate(
            columnExpression,
            typeof(TestModel).GetProperty(nameof(TestModel.Data))!,
            typeof(TestModel),
            null!);

        Assert.Null(result);
    }

    [Fact]
    public void ReturnsNull_WhenTypeMappingIsNotJson()
    {
        using var context = new TestContext(
            configureOptions: builder =>
            {
                builder.UseNpgsqlJsonSerializerOptions();
            });

        var translator = context.GetTranslator();
        var columnExpression = Substitute.For<ColumnExpression>(
            typeof(int),
            new IntTypeMapping("int"));

        var result = translator.Translate(
            columnExpression,
            typeof(TestModel).GetProperty(nameof(TestModel.Id))!,
            typeof(int),
            null!);

        Assert.Null(result);
    }

    private sealed class NullConvertName : ValueConverter<string, string>, IJsonSerializerConvertName
    {
        public NullConvertName()
            : base(static (v) => v, static (v) => v)
        {
        }

        public string? ConvertName(string name)
        {
            return null;
        }
    }
}
