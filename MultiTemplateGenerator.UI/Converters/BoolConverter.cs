using System;
using System.Globalization;
using System.Windows.Data;

namespace MultiTemplateGenerator.UI.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class BoolConverter : IValueConverter
    {
        public bool TrueValue { get; set; } = true;
        public bool FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (value is int intValue)
            //{
            //    return intValue == parameter.ToInt() ? TrueValue : FalseValue;
            //}

            if (!(value is bool))
                return FalseValue;

            return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (Equals(value, TrueValue))
                return true;
            if (Equals(value, FalseValue))
                return false;
            return null;
        }
    }
}
