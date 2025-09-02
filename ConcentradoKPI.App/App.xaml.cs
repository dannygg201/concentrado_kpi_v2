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

            // Evita que la app se cierre cuando cerremos el splash
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var splash = new SplashWindow();
            splash.Show();

            // TODO: aquí tu inicialización real (DB, config, etc.)
            await Task.Delay(3000);

            // Crea y muestra la ventana principal
            var main = new MainWindow();
            Current.MainWindow = main;        // <- establece la ventana principal
            main.Show();

            // Cierra el splash
            splash.Close();

            // Restaura el shutdown normal: la app cierra cuando se cierre MainWindow
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}
