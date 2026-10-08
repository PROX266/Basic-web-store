using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace WebApplication1.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PriceRangeAttribute : ValidationAttribute
{
    private readonly decimal _min;
    private readonly decimal _max;
    private readonly int _maxDecimals;

    public PriceRangeAttribute(double min, double max, int maxDecimals = 2)
    {
        _min = (decimal)min;
        _max = (decimal)max;
        _maxDecimals = maxDecimals;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        decimal price;
        try
        {
            price = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return ValidationHelper.Fail(validationContext, $"«{validationContext.DisplayName}» должна быть числом");
        }

        var field = validationContext.DisplayName;
        var ru = CultureInfo.CurrentCulture;

        if (price <= 0)
            return ValidationHelper.Fail(validationContext, $"«{field}» должна быть больше нуля");

        if (price < _min)
            return ValidationHelper.Fail(validationContext, $"«{field}» не может быть меньше {_min.ToString("N2", ru)}");

        if (price > _max)
            return ValidationHelper.Fail(validationContext, $"«{field}» не может быть больше {_max.ToString("N2", ru)}");

        if (Math.Round(price, _maxDecimals) != price)
            return ValidationHelper.Fail(validationContext,
                $"«{field}» должна содержать не более {_maxDecimals} знаков после запятой");

        return ValidationResult.Success;
    }
}
