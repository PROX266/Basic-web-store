using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class NotCategoryNameAttribute : ValidationAttribute
{
    private readonly string _categoryProperty;

    public NotCategoryNameAttribute(string categoryProperty)
    {
        _categoryProperty = categoryProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string name || string.IsNullOrWhiteSpace(name))
            return ValidationResult.Success;

        var categoryId = ValidationHelper.ReadInt(validationContext, _categoryProperty);
        if (categoryId == null)
            return ValidationResult.Success;

        var db = validationContext.GetService(typeof(WebstoreContext)) as WebstoreContext;
        if (db == null)
            return ValidationResult.Success;

        var categoryName = db.Categories.AsNoTracking()
            .Where(c => c.Id == categoryId)
            .Select(c => c.NameCategories)
            .FirstOrDefault();

        if (categoryName == null)
            return ValidationResult.Success;

        var a = Clean(name);
        var b = Clean(categoryName);

        if (a.Length == 0 || b.Length == 0)
            return ValidationResult.Success;

        bool similar = a == b ||
            (Math.Min(a.Length, b.Length) >= 4 &&
             Math.Abs(a.Length - b.Length) <= 2 &&
             (a.StartsWith(b) || b.StartsWith(a)));

        if (similar)
        {
            return ValidationHelper.Fail(validationContext,
                $"Название товара не должно повторять название категории «{categoryName}». Уточните, что это за товар");
        }

        return ValidationResult.Success;
    }

    private static string Clean(string text)
    {
        return Regex.Replace(ValidationHelper.Normalize(text), @"[^\p{L}]", "");
    }
}
