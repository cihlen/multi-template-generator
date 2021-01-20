using System;
using System.Globalization;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace MultiTemplateGenerator.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(ColorZoneMode))]
    public class ColorZoneDarkModeConverter : IValueConverter
    {
        public ColorZoneMode TrueValue { get; set; } = ColorZoneMode.PrimaryDark;
        public ColorZoneMode FalseValue { get; set; } = ColorZoneMode.PrimaryLight;

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
