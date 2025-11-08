using System.Threading.Tasks;
using System.Windows;
using ConcentradoKPI.App.Views;

namespace ConcentradoKPI.App
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1) Handler global para Loaded de TODAS las Window
            EventManager.RegisterClassHandler(
                typeof(Window),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(Global_WindowLoaded)
            );

            // 2) Mostramos splash sin cerrar la app al cerrar esa ventana
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var splash = new SplashWindow();
            splash.Show();

            // 3) Inicialización (simulada)
            await Task.Delay(1500);

            // 4) Abrimos la ventana principal
            var main = new MainWindow();
            Current.MainWindow = main;
            main.Show();

            // 5) Cerramos splash y restauramos shutdown normal
            splash.Close();
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void Global_WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Window w) return;

            // Evita que SizeToContent bloquee el maximizado
            if (w.SizeToContent != SizeToContent.Manual)
                w.SizeToContent = SizeToContent.Manual;

            // Solo estas 3 van maximizadas
            if (w is SplashWindow || w is MainWindow || w is ShellWindow)
            {
                w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                w.WindowState = WindowState.Maximized;
                return;
            }

            // Todas las demás: tamaño normal y centradas
            if (w.WindowState == WindowState.Maximized)
                w.WindowState = WindowState.Normal;

            w.WindowStartupLocation = (w.Owner != null)
                ? WindowStartupLocation.CenterOwner
                : WindowStartupLocation.CenterScreen;
        }
    }
}
