using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Controls;
using LinksAndMore.Models;

namespace LinksAndMore.Converters;

public class ItemTypeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ItemType type)
        {
            return type switch
            {
                ItemType.Link => SymbolRegular.Link24,
                ItemType.Note => SymbolRegular.Note24,
                ItemType.Snippet => SymbolRegular.Code24,
                _ => SymbolRegular.Question24
            };
        }
        return SymbolRegular.Question24;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
