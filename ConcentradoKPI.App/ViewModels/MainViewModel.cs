using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcentradoKPI.App.ViewModels
{
    public enum ExportScope { All, Company, Project }

    public class MainViewModel
    {
        public string Mensaje { get; set; } = "Bienvenido al Concentrado KPI";
    }
}
