using Avalonia.Data.Converters;
using Avalonia.Media;
using Pandora.Utils;
using System;
using System.Globalization;

namespace Pandora.Converter
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var flag = value is bool b && b;

            return flag ? ThemeHelper.GetBrushByName("Accent")
                        : Brushes.Transparent;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
