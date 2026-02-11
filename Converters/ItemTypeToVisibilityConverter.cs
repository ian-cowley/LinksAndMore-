using System.Globalization;
using System.Windows;
using System.Windows.Data;
using LinksAndMore.Models;

namespace LinksAndMore.Converters;

public class ItemTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ItemType type)
        {
            if (parameter?.ToString() == "OpenButton")
            {
                return type == ItemType.Link ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
