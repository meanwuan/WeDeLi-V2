// File: Extensions/EnumExtensions.cs

namespace wedeli.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// Safely parse string to enum with fallback value
    /// </summary>
    public static TEnum ParseOrDefault<TEnum>(this string? value, TEnum defaultValue)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return Enum.TryParse<TEnum>(value, true, out var result)
            ? result
            : defaultValue;
    }

    /// <summary>
    /// Convert enum to snake_case for database
    /// </summary>
    public static string ToSnakeCase(this Enum value)
    {
        var str = value.ToString();
        return string.Concat(str.Select((x, i) =>
            i > 0 && char.IsUpper(x)
                ? "_" + x.ToString().ToLower()
                : x.ToString().ToLower()));
    }

    /// <summary>
    /// Get description or friendly name
    /// </summary>
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
            .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;

        return attribute?.Description ?? value.ToString();
    }
}