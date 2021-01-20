using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MultiTemplateGenerator.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class ForegroundDarkModeConverter : IValueConverter
    {
        public Brush TrueValue { get; set; } = Brushes.White;
        public Brush FalseValue { get; set; } = SystemColors.ControlTextBrush;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return FalseValue;

            return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
