using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel; // DesignerProperties
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.Converters
{
    public class EnumToViewConverter : IValueConverter
    {
        private static readonly bool IsDesign =
            DesignerProperties.GetIsInDesignMode(new DependencyObject());

        private readonly Dictionary<AppView, UserControl> _cache = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (IsDesign)
                return new UserControl { Content = new TextBlock { Text = "Vista (diseño)", Margin = new Thickness(12) } };

            if (value is not AppView v) return null;

            if (_cache.TryGetValue(v, out var uc))
                return uc;

            uc = v switch
            {
                AppView.PersonalVigente => Create("ConcentradoKPI.App.Views.Pages.PersonalVigenteView"),
                AppView.PiramideSeguridad => Create("ConcentradoKPI.App.Views.Pages.PiramideSeguridadView"),
                AppView.InformeSemanalCma => Create("ConcentradoKPI.App.Views.Pages.InformeSemanalCmaView"),
                AppView.PrecursorSif => Create("ConcentradoKPI.App.Views.Pages.PrecursorSifView"),
                AppView.Incidentes => Create("ConcentradoKPI.App.Views.Pages.IncidentesView"),
                _ => null
            };

            if (uc != null) _cache[v] = uc;
            return uc;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static UserControl Create(string fullTypeName)
        {
            var asmName = typeof(EnumToViewConverter).Assembly.GetName().Name; // nombre real del ensamblado
            var t = Type.GetType($"{fullTypeName}, {asmName}");
            if (t == null)
            {
                return new UserControl
                {
                    Content = new TextBlock
                    {
                        Text = $"Vista no encontrada: {fullTypeName}",
                        Margin = new Thickness(12)
                    }
                };
            }
            return (UserControl)Activator.CreateInstance(t);
        }
    }
}
