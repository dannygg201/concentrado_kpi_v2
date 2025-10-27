using System.Windows;
using System.Windows.Controls;

namespace ConcentradoKPI.App.Views.Controls
{
    public partial class TopBar : UserControl
    {
        public TopBar() => InitializeComponent();

        // Opcional: permite forzar un título distinto al de la Window
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(TopBar), new PropertyMetadata(string.Empty));

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
