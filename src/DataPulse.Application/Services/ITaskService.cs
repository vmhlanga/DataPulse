using System.Collections.Generic;
using System.Threading.Tasks;
using DataPulse.Domain.Models;

namespace DataPulse.Application.Services
{
    public interface ITaskService
    {
        Task<DataTask?> GetAsync(int id);
        Task<IReadOnlyCollection<DataTask>> GetRecentAsync(int take = 20);
        Task<int> CreateAsync(DataTask task);
        Task UpdateStatusAsync(int taskId, string status, string? user);
        Task RecordRunTimesAsync(int taskId, System.DateTime start, System.DateTime? end);
    }
}
