using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql;
using XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

namespace XO.EntityFrameworkCore;

internal sealed class TestContext : DbContext
{
    private readonly Action<DbContextOptionsBuilder>? _configureOptions;

    private static readonly DbDataSource _dataSource;
    private static int _nextId = 1;

    private NpgsqlJsonMemberTranslatorPlugin? _plugin;
    private NpgsqlJsonMemberTranslator? _translator;

    static TestContext()
    {
        _dataSource = new NpgsqlDataSourceBuilder(
            "Host=localhost;Username=postgres;Password=password;Database=XO.EntityFrameworkCore.Tests;Include Error Detail=true")
            .EnableDynamicJson()
            .Build();

        using var context = new TestContext();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    public TestContext(
        Action<DbContextOptionsBuilder>? configureOptions = null)
    {
        _configureOptions = configureOptions;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(_dataSource)
            .UseNpgsqlJsonSerializerOptions()
            ;

        _configureOptions?.Invoke(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestModel>(entity =>
        {
            entity.Property(x => x.DataCustomValueComparer)
                .Metadata.SetValueComparer(new ValueComparer<TestJsonDataObject>(true));

            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            entity.Property(x => x.DataCustomValueConverter)
                .HasConversion(
                    x => JsonSerializer.Serialize(x, options),
                    x => JsonSerializer.Deserialize<TestJsonDataObject>(x, options))
                ;

            entity.Property(x => x.DataExplicit)
                .UseJsonSerializerOptions(new JsonSerializerOptions(JsonSerializerDefaults.Web))
                ;

            entity.Property(x => x.DataExplicitNoValueComparer)
                .UseJsonSerializerOptions(new JsonSerializerOptions(JsonSerializerDefaults.Web), false)
                ;
        });
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "Test")]
    public NpgsqlJsonMemberTranslatorPlugin GetPlugin()
        => _plugin ??= InfrastructureExtensions.GetService<IEnumerable<IMemberTranslatorPlugin>>(this)
            .OfType<NpgsqlJsonMemberTranslatorPlugin>()
            .Single();

    public NpgsqlJsonMemberTranslator GetTranslator()
        => _translator ??= GetPlugin().Translators
            .OfType<NpgsqlJsonMemberTranslator>()
            .Single();

    public DbSet<TestModel> TestModels => Set<TestModel>();

    public static int NextId()
        => Interlocked.Increment(ref _nextId);
}
