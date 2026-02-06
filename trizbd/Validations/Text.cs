using System.Globalization;
using System.Windows.Controls;
using Microsoft.IdentityModel.Tokens;

namespace trizbd.Validations;

public class Text:ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        var input = (value ?? "").ToString().Trim();
        
        if (input.IsNullOrEmpty()) return new ValidationResult(false, "Поле не может быть пустым");
        
        return ValidationResult.ValidResult;
    }
}