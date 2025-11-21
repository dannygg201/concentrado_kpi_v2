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

        // 🔴 Botón "Salir"
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window == null)
                return;

            // Si es la ventana principal -> cierra la app
            if (window == Application.Current.MainWindow)
            {
                Application.Current.Shutdown();
            }
            else
            {
                // Si es un ShellWindow u otra ventana hija -> solo ciérrala
                window.Close();
            }

            e.Handled = true;
        }
    }
}
