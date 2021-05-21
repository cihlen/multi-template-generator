using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MultiTemplateGenerator.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BoolParameterBrushConverter : IValueConverter
    {
        public Brush TrueValue { get; set; } = Brushes.Yellow;
        public Brush FalseValue { get; set; } = Brushes.BlueViolet;

        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool) || !(parameter is bool))
                return null;

            var isTrue = (bool)value;
            var isDarkMode = (bool)parameter;

            if (!isTrue)
            {
                return null;
            }

            return isDarkMode ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
