using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Pandora.Converter
{
    public class BoolToConnectTextConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var flag = value is bool b && b;
            return flag ? "Disconnect" : "Connect";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
