namespace ConcentradoKPI.App.Views.Pages
{
    public interface IWeekSyncPage
    {
        // Empuja los cambios visibles en la UI al WeekData (documento correspondiente)
        void SyncIntoWeek();
    }
}
