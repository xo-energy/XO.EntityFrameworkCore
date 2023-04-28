# XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions

[![NuGet](https://img.shields.io/nuget/v/XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions)](https://www.nuget.org/packages/XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions)
[![GitHub Actions Status](https://img.shields.io/github/actions/workflow/status/xo-energy/XO.EntityFrameworkCore/ci.yml?branch=main&logo=github)](https://github.com/xo-energy/XO.EntityFrameworkCore/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/xo-energy/XO.EntityFrameworkCore/branch/main/graph/badge.svg?token=V8EGOY3JV9)](https://codecov.io/gh/xo-energy/XO.EntityFrameworkCore)

An Entity Framework Core plugin that adds support for `JsonSerializerOptions` to the Npgsql provider. You could use it to influence the property naming policy, or to use JSON source generation. These features are [planned](https://github.com/dotnet/efcore/issues/30677) for Entity Framework Core 8.0, but until then...

## Usage

1. Call the extension method to add the plugin to your `DbContext`. Optionally, configure default `JsonSerializerOptions` to activate the plugin for all `json` and `jsonb` columns.

    ```csharp
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql("Host=localhost")
            .UseNpgsqlJsonSerializerOptions(defaultJsonSerializerOptions: null);
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
