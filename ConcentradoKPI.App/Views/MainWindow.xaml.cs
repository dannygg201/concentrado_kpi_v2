using System.Windows;
using ConcentradoKPI.App.ViewModels;

namespace ConcentradoKPI.App.Views   
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
