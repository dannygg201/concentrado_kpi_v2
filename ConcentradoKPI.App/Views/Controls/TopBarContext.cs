using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.Views.Controls
{
    public sealed class TopBarContext
    {
        public Company Company { get; }
        public Project Project { get; }
        public WeekData Week { get; }

        public TopBarContext(Company c, Project p, WeekData w)
        {
            Company = c;
            Project = p;
            Week = w;
        }
    }
}
