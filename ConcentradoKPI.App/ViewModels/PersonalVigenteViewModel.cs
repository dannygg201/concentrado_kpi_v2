using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.ViewModels
{
    public class PersonalVigenteViewModel
    {
        public Company Company { get; }
        public Project Project { get; }
        public WeekData Week { get; }

        public PersonalVigenteViewModel(Company company, Project project, WeekData week)
        {
            Company = company;
            Project = project;
            Week = week;
        }

        // Aquí después metemos la ObservableCollection del personal y sus comandos (Agregar/Editar/Eliminar)
    }
}
