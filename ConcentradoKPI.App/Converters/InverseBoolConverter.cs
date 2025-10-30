using System;
using System.Globalization;
using System.Windows.Data;

namespace ConcentradoKPI.App.Converters
{
    // ¡Debe ser public y con ctor por defecto!
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : Binding.DoNothing;
        }
    }
}
