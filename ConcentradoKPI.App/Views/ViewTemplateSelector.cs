using System.Windows;
using System.Windows.Controls;
using ConcentradoKPI.App.ViewModels;

namespace ConcentradoKPI.App.Views
{
    public class ViewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? DefaultTemplate { get; set; }
        public DataTemplate? TotalesTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ContratistaRow row && row.IsTotal)
                return TotalesTemplate ?? DefaultTemplate ?? base.SelectTemplate(item, container);

            return DefaultTemplate ?? base.SelectTemplate(item, container);
        }
    }
}
