using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Client.Services.Color;

/// <summary>
/// Класс конвертирования цветов
/// </summary>
[ValueConversion(typeof(WindowState), typeof(Visibility))]
public class ColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null && !string.IsNullOrEmpty(value.ToString()))
        {
            return (SolidColorBrush)new BrushConverter().ConvertFrom(value.ToString());
        }
        else
        {
            return (SolidColorBrush)new BrushConverter().ConvertFrom("#FFD9D9D9");
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}