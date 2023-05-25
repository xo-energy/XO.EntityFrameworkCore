# XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions

[![NuGet](https://img.shields.io/nuget/v/XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions)](https://www.nuget.org/packages/XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions)
[![GitHub Actions Status](https://img.shields.io/github/actions/workflow/status/xo-energy/XO.EntityFrameworkCore/ci.yml?branch=main&logo=github)](https://github.com/xo-energy/XO.EntityFrameworkCore/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/xo-energy/XO.EntityFrameworkCore/branch/main/graph/badge.svg?token=V8EGOY3JV9)](https://codecov.io/gh/xo-energy/XO.EntityFrameworkCore)

An Entity Framework Core plugin that adds support for `JsonSerializerOptions` to the Npgsql provider. You could use it to influence the property naming policy, or to use JSON source generation. These features are [planned](https://github.com/dotnet/efcore/issues/30677) for Entity Framework Core 8.0, but until then...

## Usage

1. Call the extension method to add the plugin to your `DbContext`.

    ```csharp
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql("Host=localhost")
            .UseNpgsqlJsonSerializerOptions();
    }
    ```

2. Call `UseJsonSerializerOptions` to configure `JsonSerializerOptions` for a specific property.

    ```csharp
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MyEntity>(entity =>
        {
            entity.Property(x => x.MyJsonProperty)
                .HasColumnType("jsonb")
                .UseJsonSerializerOptions(new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });
        });
    }
    ```

## Options

### Default Serializer Options

To apply default `JsonSerializerOptions` to all `json` and `jsonb` columns, pass the default instance to the options builder:

> **Note:** When configuring the default serializer options, pass the **same instance** every time your configuration callback is invoked. Otherwise, Entity Framework will detect the options have changed and construct a new `IServiceProvider` for every `DbContext`!

```csharp
private static readonly JsonSerializerOptions defaultOptions
    = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseNpgsql("Host=localhost")
        .UseNpgsqlJsonSerializerOptions(defaultOptions);
}
```

### JsonSerializerValueComparer&lt;T&gt;

By default, the plugin configures affected entity properties to use `JsonSerializerValueComparer<T>` for value equality comparison, which serializes each value to JSON and compares the resulting strings. If you prefer the default value comparer's semantics for your value type(s), or if you just don't want the additional serialization overhead, you can disable it:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseNpgsql("Host=localhost")
        .UseNpgsqlJsonSerializerOptions(useJsonSerializerValueComparer: false);
}
```
