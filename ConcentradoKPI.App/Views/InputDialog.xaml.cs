using System.Windows;

namespace ConcentradoKPI.App.Views
{
    public partial class InputDialog : Window
    {
        public string ResultText { get; private set; } = "";

        public InputDialog(string title, string prompt, string? initial = null)
        {
            InitializeComponent();
            Title = title;
            LblPrompt.Text = prompt;
            TxtValue.Text = initial ?? "";
            Loaded += (_, __) => { TxtValue.SelectAll(); TxtValue.Focus(); };
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            ResultText = TxtValue.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(ResultText))
            {
                MessageBox.Show("Escribe un nombre.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            DialogResult = true;
            Close();
        }
    }
}
