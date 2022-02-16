using System;
using System.Globalization;

using Avalonia.Data.Converters;

namespace ozz.wpf.Services;

public class BoolToStringConverter : IValueConverter {

    #region IValueConverter Members

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value switch {
            bool bv => bv ? "Da" : "Ne",
            _ => ""
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value switch {
            string s => s == "Da",
            _ => false
        };
    }

    #endregion

}