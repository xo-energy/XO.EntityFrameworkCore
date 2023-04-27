using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace XO.EntityFrameworkCore;

internal sealed class TestContext : DbContext
{
    private readonly Action<DbContextOptionsBuilder>? _configureOptions;

    private static readonly object _databaseCreatedLock = new object();
    private static bool _databaseCreated;
    private static int _nextId = 1;

    static TestContext()
    {
        lock (_databaseCreatedLock)
        {
            if (_databaseCreated)
                return;

            using var context = new TestContext();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            _databaseCreated = true;
        }
    }

    public TestContext(
        Action<DbContextOptionsBuilder>? configureOptions = null)
    {
        _configureOptions = configureOptions;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql("Host=localhost;Username=postgres;Password=password;Database=XO.EntityFrameworkCore.Tests;Include Error Detail=true")
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

    public DbSet<TestModel> TestModels => Set<TestModel>();

    public static int NextId()
        => Interlocked.Increment(ref _nextId);
}
