using System.Collections.ObjectModel;

namespace ConcentradoKPI.App.Models
{
    public class Company
    {
        public string Name { get; set; } = "";
        //public ObservableCollection<Project> Projects { get; set; } = new();
        public override string ToString() => Name;
    }
}
