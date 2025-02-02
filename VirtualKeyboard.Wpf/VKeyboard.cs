﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VirtualKeyboard.Wpf.Controls;
using VirtualKeyboard.Wpf.Types;
using VirtualKeyboard.Wpf.ViewModels;
using VirtualKeyboard.Wpf.Views;

namespace VirtualKeyboard.Wpf
{
    public static class VKeyboard
    {
        private const string _keyboardValueName = "KeyboardValueContent";
        private const string _keyboardName = "KeyboardContent";

        private static Type _hostType = typeof(DefaultKeyboardHost);

        private static TaskCompletionSource<string> _tcs;
        private static Window _windowHost;

        public static void Config(Type hostType)
        {
            if (hostType.IsSubclassOf(typeof(Window))) _hostType = hostType;
            else throw new ArgumentException();
        }

        public static void Listen<T>(Expression<Func<T, string>> property) where T: UIElement
        {
            EventManager.RegisterClassHandler(typeof(T), UIElement.PreviewMouseLeftButtonDownEvent, (RoutedEventHandler)(async (s, e) =>
            {
                if (s is AdvancedTextBox) return;
                bool acceptsReturn = false;
                bool onlyNumericInput = false;
                int maxLength = 0;
                if (s is TextBox src)
                {
                    acceptsReturn = src.AcceptsReturn;
                    onlyNumericInput = "numeric".Equals(src.Tag);
                    maxLength = src.MaxLength;
                }
                var memberSelectorExpression = property.Body as MemberExpression;
                if (memberSelectorExpression == null) return;
                var prop = memberSelectorExpression.Member as PropertyInfo;
                if (prop == null) return;
                var initValue = (string)prop.GetValue(s);
                var value = await OpenAsync(initValue, acceptsReturn, onlyNumericInput, maxLength);
                prop.SetValue(s, value, null);
            }));
        }

        public static Task<string> OpenAsync(string initialValue = "", bool acceptsReturn = false,  bool onlyNumericInput = false, int maxLength = 0)
        {
            if (_windowHost != null) throw new InvalidOperationException();


            _tcs = new TaskCompletionSource<string>();
            _windowHost = (Window)Activator.CreateInstance(_hostType);
            _windowHost.DataContext = new VirtualKeyboardViewModel(initialValue, acceptsReturn, onlyNumericInput, maxLength);
            ((ContentControl)_windowHost.FindName(_keyboardValueName)).Content = new KeyboardValueView();
            ((ContentControl)_windowHost.FindName(_keyboardName)).Content = new VirtualKeyboardView();
            void handler(object s, CancelEventArgs a)
            {
                var result = GetResult();
                ((Window)s).Closing -= handler;
                _windowHost = null;
                _tcs?.SetResult(result);
                _tcs = null;
            }

            _windowHost.Closing += handler;

            _windowHost.Owner = GetMainWindow();
            _windowHost.Show();
            return _tcs.Task;
        }
        private static Window GetMainWindow()
        {
            try
            {
                WindowCollection windows = Application.Current.Windows;
                foreach (Window window in windows)
                {
                    if (window.IsVisible && "MainWindow".Equals(window.Tag))
                    {
                        return window;
                    }
                }
            }
            catch (Exception _e)
            {
                //Debug.WriteLine(_e.Message);
            }
            return Application.Current.MainWindow;
        }

        public static void Close()
        {
            if (_windowHost == null) throw new InvalidOperationException();

            _windowHost.Close();
        }

        private static string GetResult()
        {
            var viewModel = (VirtualKeyboardViewModel)_windowHost.DataContext;
            return viewModel.KeyboardText;
        }
    }
}
