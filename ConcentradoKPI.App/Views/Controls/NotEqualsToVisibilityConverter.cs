// Views/Controls/NotEqualsToVisibilityConverter.cs
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.Views.Controls
{
    public class NotEqualsToVisibilityConverter : IValueConverter
    {
        public static readonly NotEqualsToVisibilityConverter Instance = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AppView current && parameter is AppView target)
                return current == target ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Visible;
        }
        public object ConvertBack(object value, Type t, object p, CultureInfo c) => throw new NotSupportedException();
    }
}
