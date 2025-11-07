using System.Globalization;
using System.Windows.Data;

namespace ConcentradoKPI.App.Converters
{
    public class BoolToSiNoConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? (b ? "Sí" : "No") : "";

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
            => value is string s && s.Trim().ToLowerInvariant() == "sí";
    }
}