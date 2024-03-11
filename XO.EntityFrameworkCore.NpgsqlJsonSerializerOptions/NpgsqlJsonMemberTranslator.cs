using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace XO.EntityFrameworkCore.NpgsqlJsonSerializerOptions;

#pragma warning disable EF1001 // Internal EF Core API usage.
internal sealed class NpgsqlJsonMemberTranslator : NpgsqlJsonPocoTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

    public NpgsqlJsonMemberTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        NpgsqlSqlExpressionFactory sqlExpressionFactory,
        IModel model)
        : base(typeMappingSource, sqlExpressionFactory, model)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    public override SqlExpression? Translate(SqlExpression? instance, MemberInfo member, Type returnType, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        switch (instance)
        {
            case ColumnExpression columnExpression:
                return TranslateJsonTypeMapping(instance, columnExpression, member, returnType);

            case PgJsonTraversalExpression { Expression: ColumnExpression columnExpression }:
                return TranslateJsonTypeMapping(instance, columnExpression, member, returnType);

            default:
                return null;
        }
    }

    private SqlExpression? TranslateJsonTypeMapping(SqlExpression instance, ColumnExpression columnExpression, MemberInfo member, Type returnType)
    {
        if (columnExpression.TypeMapping is not NpgsqlJsonTypeMapping jsonTypeMapping)
            return null;

        if (jsonTypeMapping.Converter is not IJsonSerializerConvertName converter)
            return null;

        // defer to the base class's handling of 'Count'
        if (member.Name == nameof(List<object>.Count)
            && member.DeclaringType?.IsGenericType == true
            && member.DeclaringType.GetGenericTypeDefinition() == typeof(List<>))
        {
            return TranslateArrayLength(instance);
        }

        // defer to JsonPropertyNameAttribute
        var jsonName = member.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;

        // use the configured converter's naming policy
        jsonName ??= converter.ConvertName(member.Name);

        if (jsonName != null)
        {
            return TranslateMemberAccess(
                instance,
                _sqlExpressionFactory.Constant(jsonName),
                returnType);
        }

        return null;
    }
}
#pragma warning restore EF1001 // Internal EF Core API usage.
