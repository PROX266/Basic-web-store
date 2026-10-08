using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Validation;

internal static class ValidationHelper
{
    public static ValidationResult Fail(ValidationContext context, string message)
    {
        return context.MemberName == null
            ? new ValidationResult(message)
            : new ValidationResult(message, new[] { context.MemberName });
    }

    public static int? ReadInt(ValidationContext context, string propertyName)
    {
        var property = context.ObjectType.GetProperty(propertyName);
        return property?.GetValue(context.ObjectInstance) as int?;
    }

    public static string Normalize(string text)
    {
        var chars = text.ToLowerInvariant().Replace('ё', 'е').ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = chars[i] switch
            {
                'a' => 'а',
                'e' => 'е',
                'o' => 'о',
                'p' => 'р',
                'c' => 'с',
                'x' => 'х',
                'y' => 'у',
                _ => chars[i]
            };
        }
        return new string(chars);
    }
}
