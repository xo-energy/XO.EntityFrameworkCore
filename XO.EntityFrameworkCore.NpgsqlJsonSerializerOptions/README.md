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
