﻿using System;
using System.Globalization;
using System.Windows.Data;
using VirtualKeyboard.Wpf.Types;
using VirtualKeyboard.Wpf.Views;

namespace VirtualKeyboard.Wpf.Converters
{
    class KeyboardTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (KeyboardType)value;
            switch (type)
            {
                case KeyboardType.Alphabet: return (culture?.TwoLetterISOLanguageName?.Equals("de")??false) ? new AlphabetViewDE() : new AlphabetView();
                case KeyboardType.Special: return new SpecialCharactersView();
                case KeyboardType.Numeric: return new NumericView();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
