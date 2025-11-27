using System;
using System.Globalization;
using System.Windows.Data;

namespace ConcentradoKPI.App.Converters
{
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? "Activo" : "Baja";

            return "Baja";
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                s = s.Trim().ToLowerInvariant();
                if (s == "activo") return true;
                if (s == "baja") return false;
            }
            return false;
        }
    }
}
