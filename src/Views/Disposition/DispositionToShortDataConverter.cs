using System;
using System.Globalization;

using Avalonia.Data.Converters;

namespace ozz.wpf.Views.Disposition;

public class DispositionToShortDataConverter : IValueConverter {

    #region IValueConverter Members

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is Models.Disposition d) {
            //return $"{d.Category[0]}    X{d.PlayCountRemaining}";
            return $"{d.Category[0]}";
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value;
    }

    #endregion

}