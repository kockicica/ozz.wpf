using System;
using System.Globalization;

using Avalonia.Data.Converters;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public class ToIdConverter : IValueConverter {

    #region IValueConverter Members

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value switch {
            HasId hasId => hasId.Id,
            _ => null
        };
    }

    #endregion

}