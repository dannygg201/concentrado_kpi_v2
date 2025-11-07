using System;
using System.Globalization;
using System.Windows.Data;

namespace ConcentradoKPI.App.Converters
{
    public class EqualsToDisabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return true; // Por defecto habilitado

            // 🔹 Devuelve false (deshabilitado) si el valor actual coincide con el parámetro
            return !Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
