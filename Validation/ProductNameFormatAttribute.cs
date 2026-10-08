using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebApplication1.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ProductNameFormatAttribute : ValidationAttribute
{
    private const string AllowedSymbols = " -.,+/()\"'&%№#:";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text || text.Length == 0)
            return ValidationResult.Success;

        var field = validationContext.DisplayName;

        if (text != text.Trim())
            return ValidationHelper.Fail(validationContext, $"«{field}» не должно начинаться или заканчиваться пробелом");

        if (text.Contains("  "))
            return ValidationHelper.Fail(validationContext, $"«{field}» не должно содержать подряд идущие пробелы");

        if (!char.IsLetterOrDigit(text[0]))
            return ValidationHelper.Fail(validationContext, $"«{field}» должно начинаться с буквы или цифры");

        foreach (var c in text)
        {
            if (!char.IsLetterOrDigit(c) && !AllowedSymbols.Contains(c))
                return ValidationHelper.Fail(validationContext, $"«{field}» содержит недопустимый символ «{c}»");
        }

        var letters = text.Where(char.IsLetter).ToList();

        if (letters.Count < 2)
            return ValidationHelper.Fail(validationContext, $"«{field}» должно содержать не менее двух букв");

        if (letters.Count >= 4 && letters.All(char.IsUpper))
            return ValidationHelper.Fail(validationContext, $"«{field}» не нужно писать только заглавными буквами");

        if (Regex.IsMatch(text, @"(.)\1{3,}"))
            return ValidationHelper.Fail(validationContext, $"«{field}» содержит один и тот же символ более трёх раз подряд");

        if (Regex.IsMatch(text, @"[!?]{2,}"))
            return ValidationHelper.Fail(validationContext, $"«{field}» не должно содержать несколько знаков «!» или «?» подряд");

        return ValidationResult.Success;
    }
}
