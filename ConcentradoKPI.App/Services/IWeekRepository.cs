// Services/IWeekRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.Services
{
    public interface IWeekRepository
    {
        Task<WeekData?> GetLastWeekAsync(int companyId, int projectId, int beforeYear, int beforeWeek);
        Task SaveAsync(WeekData week);
        Task<IReadOnlyList<WeekData>> GetByProjectAsync(int companyId, int projectId);
    }
}
