using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MultiTemplateGenerator.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class MultiBoolBrushConverter : IMultiValueConverter
    {
        public Brush TrueValue { get; set; } = Brushes.Yellow;
        public Brush FalseValue { get; set; } = Brushes.BlueViolet;

        
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return null;

            var isDarkMode = (bool) values[0];
            var isTrue = (bool)values[1];

            if (!isTrue)
            {
                return null;
            }

            return isDarkMode ? TrueValue : FalseValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw null;
        }
    }
}
