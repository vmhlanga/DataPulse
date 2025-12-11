using System.Collections.Generic;
using System.Threading.Tasks;
using DataPulse.Domain.Models;

namespace DataPulse.Application.Services
{
    public interface IProcessService
    {
        Task<Process?> GetAsync(int id);
        Task<IReadOnlyCollection<Process>> GetByTaskAsync(int taskId);
        Task UpdateStatusAsync(int processId, string status, string? error = null, string? user = null);
        Task RecordRunTimesAsync(int processId, System.DateTime start, System.DateTime? end);
    }
}
