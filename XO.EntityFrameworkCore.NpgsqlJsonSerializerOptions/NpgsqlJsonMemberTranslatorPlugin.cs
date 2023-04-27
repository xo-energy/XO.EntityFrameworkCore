using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

/// <summary>
/// Configures Entity Framework Core to use <see cref="NpgsqlJsonMemberTranslator"/>.
/// </summary>
public sealed class NpgsqlJsonMemberTranslatorPlugin : IMemberTranslatorPlugin
{
    /// <summary>
    /// Initializes a new instance of <see cref="NpgsqlJsonMemberTranslatorPlugin"/>.
    /// </summary>
    /// <param name="typeMappingSource">The type mapping source (a dependency of the underlying Npgsql type).</param>
    /// <param name="sqlExpressionFactory">A factory for creating <see cref="SqlExpression"/> instances. Assumed to be an instance of <see cref="NpgsqlSqlExpressionFactory"/>.</param>
    /// <param name="model">The entity model (a dependency of the underlying Npgsql type).</param>
    public NpgsqlJsonMemberTranslatorPlugin(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory,
        IModel model)
    {
        var translators = new List<IMemberTranslator>(1);

        if (sqlExpressionFactory is NpgsqlSqlExpressionFactory npgsqlSqlExpressionFactory)
        {
            var npgsqlJsonMemberTranslator = new NpgsqlJsonMemberTranslator(
                typeMappingSource,
                npgsqlSqlExpressionFactory,
                model);

            translators.Add(npgsqlJsonMemberTranslator);
        }

        this.Translators = translators;
    }

    /// <inheritdoc/>
    public IEnumerable<IMemberTranslator> Translators { get; }
}
