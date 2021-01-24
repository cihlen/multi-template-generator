using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MultiTemplateGenerator.UI.Converters
{
    [ValueConversion(typeof(MessageBoxButton), typeof(Visibility))]
    public class MessageBoxButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Visible;

            var isVisible = false;
            var msgBoxButton = (MessageBoxButton)value;
            var shouldBeButton = (MessageBoxResult)parameter;
            switch (msgBoxButton)
            {
                case MessageBoxButton.OK:
                    isVisible = shouldBeButton == MessageBoxResult.OK;
                    break;
                case MessageBoxButton.OKCancel:
                    isVisible = shouldBeButton == MessageBoxResult.OK || shouldBeButton == MessageBoxResult.Cancel;
                    break;
                case MessageBoxButton.YesNo:
                    isVisible = shouldBeButton == MessageBoxResult.Yes || shouldBeButton == MessageBoxResult.No;
                    break;
                case MessageBoxButton.YesNoCancel:
                    isVisible = shouldBeButton == MessageBoxResult.Yes || shouldBeButton == MessageBoxResult.No || shouldBeButton == MessageBoxResult.Cancel;
                    break;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
