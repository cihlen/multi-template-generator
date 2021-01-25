using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using MultiTemplateGenerator.UI.Helpers;
using Icon = System.Drawing.Icon;

namespace MultiTemplateGenerator.UI.ViewModels
{
    public class MessageBoxViewModel
    {
        private MessageBoxImage _messageBoxImage;
        private RelayCommand<MessageBoxResult> _buttonCommand;
        private RelayCommand _copyMessageCommand;

        public MessageBoxViewModel(string message, string title,
            MessageBoxButton messageBoxButton = MessageBoxButton.OK, MessageBoxImage messageBoxImage = MessageBoxImage.Information)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                title = messageBoxImage == MessageBoxImage.Error
                    ? "Error " + AppHelper.ProductName
                    : AppHelper.ProductName;
            }

            Message = message;
            Title = title;
            MessageBoxButton = messageBoxButton;
            MessageBoxImage = messageBoxImage;
        }

        public string Message { get; }
        public string Title { get; }
        public MessageBoxButton MessageBoxButton { get; }
        public ImageSource MessageIconSource { get; private set; }
        public MessageBoxResult MessageBoxResult { get; private set; }

        public MessageBoxImage MessageBoxImage
        {
            get => _messageBoxImage;
            set
            {
                _messageBoxImage = value;

                Icon icon = null;
                switch (_messageBoxImage)
                {
                    case MessageBoxImage.Information:
                        icon = SystemIcons.Information;
                        break;
                    case MessageBoxImage.Warning:
                        icon = SystemIcons.Warning;
                        break;
                    case MessageBoxImage.Error:
                        icon = SystemIcons.Error;
                        break;
                    case MessageBoxImage.Question:
                        icon = SystemIcons.Question;
                        break;
                }

                MessageIconSource = icon != null ? Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()) : null;
            }
        }

        public RelayCommand<MessageBoxResult> ButtonCommand => _buttonCommand ??= new RelayCommand<MessageBoxResult>((button) =>
        {
            MessageBoxResult = button;
            DialogHost.Close(ViewNames.MessageBoxDialogRoot);
        }, (button) => button != MessageBoxResult.None);


        public RelayCommand CopyMessageCommand => _copyMessageCommand ??= new RelayCommand(() =>
        {
            try
            {
                Clipboard.SetText(Message);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, null, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }, () => true);
    }
}
