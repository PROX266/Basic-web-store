using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class UniqueProductNameAttribute : ValidationAttribute
{
    private readonly string _categoryProperty;
    private readonly string _idProperty;

    public UniqueProductNameAttribute(string categoryProperty, string idProperty = "Id")
    {
        _categoryProperty = categoryProperty;
        _idProperty = idProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string name || string.IsNullOrWhiteSpace(name))
            return ValidationResult.Success;

        var categoryId = ValidationHelper.ReadInt(validationContext, _categoryProperty);
        var id = ValidationHelper.ReadInt(validationContext, _idProperty) ?? 0;

        if (categoryId == null)
            return ValidationResult.Success;

        var db = validationContext.GetService(typeof(WebstoreContext)) as WebstoreContext;
        if (db == null)
            return ValidationResult.Success;

        var trimmed = name.Trim();
        var exists = db.Products.AsNoTracking()
            .Any(p => p.CategoryId == categoryId && p.Id != id && p.ProductName == trimmed);

        if (exists)
            return ValidationHelper.Fail(validationContext, $"Товар «{trimmed}» уже есть в выбранной категории");

        return ValidationResult.Success;
    }
}
