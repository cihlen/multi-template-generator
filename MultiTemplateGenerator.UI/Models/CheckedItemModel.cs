using System;
using CommunityToolkit.Mvvm.ComponentModel;

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
                OnPropertyChanged();

                if (!string.IsNullOrEmpty(Text))
                    _checkedChanged?.Invoke(this);
            }
        }

        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
