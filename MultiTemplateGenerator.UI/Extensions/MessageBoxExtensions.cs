using System;
using System.Windows;

namespace MultiTemplateGenerator.UI
{
    public static class MessageBoxExtensions
    {
        public static void ShowInfo(this string text, string title)
        {
            MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public static void ShowWarning(this string text, string title)
        {
            MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public static void ShowErrorMessageBox(this string text)
        {
            MessageBox.Show(text, null, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowErrorMessageBox(this Exception exception)
        {
            MessageBox.Show(exception.Message, null, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool ShowQuestion(this string text, string title, MessageBoxImage mbImage = MessageBoxImage.Exclamation)
        {
            return MessageBox.Show(text, title, MessageBoxButton.YesNo, mbImage) == MessageBoxResult.Yes;
        }
    }
}
