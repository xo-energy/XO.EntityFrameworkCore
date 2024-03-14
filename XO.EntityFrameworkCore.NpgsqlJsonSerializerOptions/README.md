# XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions

[![NuGet](https://img.shields.io/nuget/v/XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions)](https://www.nuget.org/packages/XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions)
[![GitHub Actions Status](https://img.shields.io/github/actions/workflow/status/xo-energy/XO.EntityFrameworkCore/ci.yml?branch=main&logo=github)](https://github.com/xo-energy/XO.EntityFrameworkCore/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/xo-energy/XO.EntityFrameworkCore/branch/main/graph/badge.svg?token=V8EGOY3JV9)](https://codecov.io/gh/xo-energy/XO.EntityFrameworkCore)

An Entity Framework Core plugin that adds support for `JsonSerializerOptions` to the Npgsql provider's built-in JSON POCO mapping. You could use it to influence the property naming policy, or to use JSON source generation.

> **NOTE:** Version 8.0 of the Npgsql EF provider introduced full support for Entity Framework Core's JSON columns feature. This plugin is for Npgsql's native POCO mapping, not the EF Core feature. You probably want to use JSON columns in new projects!
>
> - <https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/whatsnew#json-columns>
> - <https://www.npgsql.org/efcore/mapping/json.html?tabs=data-annotations%2Cpoco>

## Usage

1. Opt in to Npgsql's dynamic JSON POCO mapping, which is [disabled by default](https://www.npgsql.org/efcore/release-notes/8.0.html#json-poco-and-other-dynamic-features-now-require-an-explicit-opt-in) in Npgsql 8.0. An example from this project's tests:

    ```csharp
    private static readonly DbDataSource _dataSource = new NpgsqlDataSourceBuilder(
        "Host=localhost;Username=postgres;Password=password;Database=XO.EntityFrameworkCore.Tests")
        .EnableDynamicJson()
        .Build();
    ```

2. Call the extension method to add the plugin to your `DbContext`.

    ```csharp
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(_dataSource)
            .UseNpgsqlJsonSerializerOptions();
    }
    ```

3. Call `UseJsonSerializerOptions` to configure `JsonSerializerOptions` for a specific property.

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
