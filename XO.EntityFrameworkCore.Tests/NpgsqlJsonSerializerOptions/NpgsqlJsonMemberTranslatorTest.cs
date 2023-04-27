using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;

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
}
