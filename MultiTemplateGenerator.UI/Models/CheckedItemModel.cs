using System;
using GalaSoft.MvvmLight;

namespace MultiTemplateGenerator.UI.Models
{
    public class CheckedItemModel : ObservableObject, IChecked
    {
        private readonly Action<CheckedItemModel> _checkedChanged;

        public CheckedItemModel()
        {
        }

        public CheckedItemModel(Action<CheckedItemModel> checkedChanged, string text, bool isChecked = false)
        {
            _checkedChanged = checkedChanged;
            _text = text;
            _isChecked = isChecked;
        }

        private bool _isChecked;
        private string _text;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value)
                    return;

                _isChecked = value;
                RaisePropertyChanged();

                if (!string.IsNullOrEmpty(Text))
                    _checkedChanged?.Invoke(this);
            }
        }

        public string Text
        {
            get => _text;
            set { _text = value; RaisePropertyChanged(); }
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
