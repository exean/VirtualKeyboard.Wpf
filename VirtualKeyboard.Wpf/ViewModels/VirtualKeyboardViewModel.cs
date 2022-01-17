using System.ComponentModel;
using VirtualKeyboard.Wpf.Types;

namespace VirtualKeyboard.Wpf.ViewModels
{
    class VirtualKeyboardViewModel : INotifyPropertyChanged
    {
        private bool _acceptsReturn;
        public bool AcceptsReturn
        {
            get => _acceptsReturn;
            set
            {
                _acceptsReturn = value;
                NotifyPropertyChanged(nameof(AcceptsReturn));
            }
        }
        private int _maxLength;
        public int maxLength
        {
            get => _maxLength;
            set
            {
                _maxLength = value;
                NotifyPropertyChanged(nameof(maxLength));
            }
        }
        private string _keyboardText;
        public string KeyboardText {
            get => _keyboardText;
            set
            {
                _keyboardText = value;
                NotifyPropertyChanged(nameof(KeyboardText));
            }
        }
        private KeyboardType _keyboardType;
        public KeyboardType KeyboardType {
            get => _keyboardType; 
            private set
            {
                _keyboardType = value;
                NotifyPropertyChanged(nameof(KeyboardType));
            }
        }
        private bool _uppercase;
        public bool Uppercase
        {
            get => _uppercase;
            private set
            {
                _uppercase = value;
                NotifyPropertyChanged(nameof(Uppercase));
            }
        }
        private int _caretPosition;
        public int CaretPosition
        {
            get => _caretPosition;
            set
            {
                if (value < 0) _caretPosition = 0;
                else if (value > KeyboardText.Length) _caretPosition = KeyboardText.Length;
                else _caretPosition = value;
                NotifyPropertyChanged(nameof(CaretPosition));
            }
        }
        private string _selectedValue;
        public string SelectedValue
        {
            get => _selectedValue;
            set
            {
                _selectedValue = value;
                NotifyPropertyChanged(nameof(SelectedValue));
            }
        }
        public Command AddCharacter { get; }
        public Command ChangeCasing { get; }
        public Command RemoveCharacter { get; }
        public Command ChangeKeyboardType { get; }
        public Command Linebreak { get; }
        public Command Accept { get; }

        public VirtualKeyboardViewModel(string initialValue, bool acceptsReturn, bool onlyNumeric, int maxLength)
        {
            _acceptsReturn = acceptsReturn;
            _keyboardText = initialValue;
            _maxLength = maxLength;
            _keyboardType = onlyNumeric ? KeyboardType.Numeric : KeyboardType.Alphabet;
            _uppercase = false;
            CaretPosition = _keyboardText.Length;

            AddCharacter = new Command(a =>
            {
                if (a is string character)
                    if (character.Length == 1)
                    {
                        if (Uppercase) character = character.ToUpper();
                        if (maxLength > 0 && KeyboardText.Length >= maxLength)
                            KeyboardText = KeyboardText.Substring(0, maxLength - 1);
                        if (!string.IsNullOrEmpty(SelectedValue))
                        {
                            RemoveSubstring(SelectedValue);
                            KeyboardText = KeyboardText.Insert(CaretPosition, character);
                            CaretPosition++;
                            SelectedValue = "";
                        }
                        else
                        {
                            KeyboardText = KeyboardText.Insert(CaretPosition, character);
                            CaretPosition++;
                        }
                    }
            });
            ChangeCasing = new Command(a => Uppercase = !Uppercase);
            RemoveCharacter = new Command(a =>
            {
                if(!string.IsNullOrEmpty(SelectedValue))
                {
                    RemoveSubstring(SelectedValue);
                }
                else
                {
                    var position = CaretPosition - 1;
                    if (position >= 0)
                    {
                        KeyboardText = KeyboardText.Remove(position, 1);
                        if (position < KeyboardText.Length) CaretPosition--;
                        else CaretPosition = KeyboardText.Length;
                    }
                }
            });
            ChangeKeyboardType = new Command(a =>
            {
                if (KeyboardType == KeyboardType.Alphabet) KeyboardType = KeyboardType.Special;
                else KeyboardType = KeyboardType.Alphabet;
            });
            Linebreak = new Command(a =>
            { 
                    if (!string.IsNullOrEmpty(SelectedValue))
                    {
                        RemoveSubstring(SelectedValue);
                        KeyboardText = KeyboardText.Insert(CaretPosition, "\n");
                        CaretPosition++;
                        SelectedValue = "";
                    }
                    else
                    {
                        KeyboardText = KeyboardText.Insert(CaretPosition, "\n");
                        CaretPosition++;                    
                    } 
            });
            Accept = new Command(a => VKeyboard.Close());
        }

        private void RemoveSubstring(string substring)
        {
            var position = KeyboardText.IndexOf(substring);
            KeyboardText = KeyboardText.Remove(position, substring.Length);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
