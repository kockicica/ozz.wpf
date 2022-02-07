using System;
using System.Globalization;

using Avalonia.Data.Converters;

namespace ozz.wpf.Services;

public class DurationToStringConverter : IValueConverter {

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value == null) {
            return string.Empty;
        }

        if (!double.TryParse(value.ToString(), out double dv)) return string.Empty;

        var d = TimeSpan.FromMilliseconds(dv / 1_000_000);
        return $"{d.Hours:00}:{d.Minutes:00}:{d.Seconds:00}";

    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        var ts = TimeSpan.Zero;
        
        if (value != null && TimeSpan.TryParse(value.ToString(), NumberFormatInfo.InvariantInfo, out var tst)) {
            ts = tst;
        }

        return ts.TotalMilliseconds * 1_000_000;

    }


}