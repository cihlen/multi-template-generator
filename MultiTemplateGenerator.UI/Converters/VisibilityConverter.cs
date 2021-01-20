using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MultiTemplateGenerator.Lib;

namespace MultiTemplateGenerator.UI.Converters
{
    [ValueConversion(typeof(object), typeof(object))]
    public class VisibilityConverter : IValueConverter
    {
        [DefaultValue(Visibility.Visible)]
        public Visibility TrueValue { get; set; } = Visibility.Visible;

        [DefaultValue(Visibility.Collapsed)]
        public Visibility FalseValue { get; set; } = Visibility.Collapsed;

        [DefaultValue(1)]
        public int MinLength { get; set; } = 1;

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value == null)
                return FalseValue;

            //If boolean
            if (value is bool b)
                return b ? TrueValue : FalseValue;

            //If a string
            if (value is string s)
            {
                //if (MinLength > 1)
                //{
                //    Debug.WriteLine("MinLength = " + MinLength);
                //}
                return !string.IsNullOrWhiteSpace(s) && s.Length >= MinLength ? TrueValue : FalseValue;
            }

            //If Visibility
            if (value is Visibility visibility)
                return visibility == TrueValue;

            if (value is int i)
            {
                if (parameter != null)
                    return i == parameter.ToInt() ? TrueValue : FalseValue;
                return i.ToBool() ? TrueValue : FalseValue;
            }

            if (value is DateTime dt)
                return dt > DateTime.MinValue.AddDays(1);

            throw new InvalidEnumArgumentException(@"Unsupported value data type: " + nameof(value));
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
