using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

public sealed class SaveChangesTest
{
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new();

    private static TestContext CreateContext(JsonSerializerOptions? options = default, bool useJsonSerializerValueComparer = true)
        => new TestContext(
            configureOptions: optionsBuilder =>
            {
                optionsBuilder.UseNpgsqlJsonSerializerOptions(
                    options,
                    useJsonSerializerValueComparer);
            });

    [Fact]
    public void CanSaveChanges_WithDefaultOptions()
    {
        using var context = CreateContext(DefaultJsonSerializerOptions);

        _ = CreateTestModelAndSaveChanges(context, out var affected);

        Assert.Equal(1, affected);
    }

    [Fact]
    public void CanSaveChanges_WithoutDefaultOptions()
    {
        using var context = CreateContext();

        _ = CreateTestModelAndSaveChanges(context, out var affected);

        Assert.Equal(1, affected);
    }

    [Fact]
    public void SaveChangesAffects0_WithDefaultOptions()
    {
        using var context = CreateContext(DefaultJsonSerializerOptions, true);

        var record = CreateTestModelAndSaveChanges(context, out _);

        record.Data = record.Data! with { Aliases = new List<string>(record.Data.Aliases!) };
        record.DataExplicit = record.DataExplicit! with { Aliases = new List<string>(record.DataExplicit.Aliases!) };

        var affected = context.SaveChanges();

        Assert.Equal(0, affected);
    }

    [Fact]
    public void SaveChangesAffects1_WithDefaultOptions_WhenUseValueComparerIsFalse()
    {
        using var context = CreateContext(DefaultJsonSerializerOptions, false);

        var record = CreateTestModelAndSaveChanges(context, out _);

        record.Data = record.Data! with { Aliases = new List<string>(record.Data.Aliases!) };
        record.DataExplicit = record.DataExplicit! with { Aliases = new List<string>(record.DataExplicit.Aliases!) };

        var affected = context.SaveChanges();

        Assert.Equal(1, affected);
    }

    [Fact]
    public void SaveChangesAffects1_WithoutDefaultOptions()
    {
        using var context = CreateContext();

        var record = CreateTestModelAndSaveChanges(context, out _);

        record.Data = record.Data! with { Aliases = new List<string>(record.Data.Aliases!) };
        record.DataExplicit = record.DataExplicit! with { Aliases = new List<string>(record.DataExplicit.Aliases!) };

        var affected = context.SaveChanges();

        Assert.Equal(1, affected);
    }

    private static TestModel CreateTestModel()
        => new()
        {
            Data = new TestJsonDataObject(nameof(TestModel.Data), 1, Aliases: new() { "Bits" }),
            DataExplicit = new TestJsonDataObject(nameof(TestModel.DataExplicit), 2, Aliases: new() { "Bobs" }),
            DataExplicitNoValueComparer = new TestJsonDataObject(nameof(TestModel.DataExplicitNoValueComparer), 3),
            DataCustomValueComparer = new TestJsonDataObject(nameof(TestModel.DataCustomValueComparer), 4),
            DataCustomValueConverter = new TestJsonDataObject(nameof(TestModel.DataCustomValueConverter), 5),
        };

    private static TestModel CreateTestModelAndSaveChanges(TestContext context, out int affected)
    {
        var record = CreateTestModel();

        context.TestModels.Add(record);
        affected = context.SaveChanges();

        return record;
    }
}
