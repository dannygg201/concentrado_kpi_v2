using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ConcentradoKPI.App.Models
{
    public class Person : INotifyPropertyChanged
    {
        private int _number;
        public int Number { get => _number; set { if (_number != value) { _number = value; OnPropertyChanged(); } } }

        private string _name = "";
        public string Name { get => _name; set { if (_name != value) { _name = value; OnPropertyChanged(); } } }

        private string? _affiliation;
        public string? Affiliation { get => _affiliation; set { if (_affiliation != value) { _affiliation = value; OnPropertyChanged(); } } }

        private string? _position;
        public string? Position { get => _position; set { if (_position != value) { _position = value; OnPropertyChanged(); } } }

        private int _hoursPerWeek;
        public int HoursPerWeek { get => _hoursPerWeek; set { if (_hoursPerWeek != value) { _hoursPerWeek = value; OnPropertyChanged(); } } }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
