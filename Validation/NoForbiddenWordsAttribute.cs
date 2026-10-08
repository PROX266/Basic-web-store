using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebApplication1.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class NoForbiddenWordsAttribute : ValidationAttribute
{
    private readonly string[] _stems;

    public NoForbiddenWordsAttribute(params string[] stems)
    {
        _stems = stems.Select(ValidationHelper.Normalize).ToArray();
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
            return ValidationResult.Success;

        var original = Regex.Split(text, @"[^\p{L}\p{N}]+").Where(w => w.Length > 0);

        foreach (var word in original)
        {
            var normalized = ValidationHelper.Normalize(word);
            if (_stems.Any(stem => normalized.StartsWith(stem)))
            {
                return ValidationHelper.Fail(validationContext,
                    $"Поле «{validationContext.DisplayName}» содержит недопустимое слово «{word}»");
            }
        }

        return ValidationResult.Success;
    }
}
