using System.Windows;
using System.Windows.Controls;

namespace ConcentradoKPI.App.Views.Controls
{
    public partial class TopBar : UserControl
    {
        public TopBar() => InitializeComponent();

        // Permite forzar un título distinto al de la Window
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(TopBar), new PropertyMetadata(string.Empty));

        public IInputElement CommandTarget
        {
            get => (IInputElement)GetValue(CommandTargetProperty);
            set => SetValue(CommandTargetProperty, value);
        }
        public static readonly DependencyProperty CommandTargetProperty =
            DependencyProperty.Register(nameof(CommandTarget), typeof(IInputElement), typeof(TopBar), new PropertyMetadata(null));

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
