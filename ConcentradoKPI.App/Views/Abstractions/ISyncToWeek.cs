namespace ConcentradoKPI.App.Views.Abstractions
{
    public interface ISyncToWeek
    {
        /// <summary>
        /// Copia los datos de la vista al objeto WeekData (solo en memoria).
        /// Suele llamarse al cambiar de pestaña / vista.
        /// </summary>
        void SyncIntoWeek();

        /// <summary>
        /// Se llama justo antes de guardar para que la vista termine
        /// de empujar todo al WeekData / Live / eventos, etc.
        /// </summary>
        void FlushToWeek();
    }
}
