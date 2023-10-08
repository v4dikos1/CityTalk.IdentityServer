using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Persistence.Extensions;

public static class SnakeCaseExtensions
{
    public static void ConvertTableNameToSnakeCase(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName().ToSnakeCase());
        }
    }

    public static string? ToSnakeCase(this string? input)
    {
        if (string.IsNullOrEmpty(input)) { return input; }

        var startUnderscores = Regex.Match(input, @"^_+");
        return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }
}