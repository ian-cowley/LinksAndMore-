using System.Globalization;
using System.Windows.Data;

namespace LinksAndMore.Converters;

public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        string checkValue = value.ToString() ?? string.Empty;
        string targetValue = parameter.ToString() ?? string.Empty;

        return checkValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter != null)
        {
            if (targetType == typeof(string))
            {
                return parameter.ToString() ?? string.Empty;
            }
            
            if (targetType.IsEnum)
            {
                var parameterText = parameter.ToString();
                if (!string.IsNullOrWhiteSpace(parameterText) && Enum.TryParse(targetType, parameterText, out var parsed))
                {
                    return parsed;
                }
            }
        }

        return Binding.DoNothing;
    }
}
