using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPulse.Application.Services;
using DataPulse.Domain.Models;
using DataPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataPulse.Infrastructure.Services
{
    public class ProcessService : IProcessService
    {
        private readonly DataPulseDbContext _dbContext;

        public ProcessService(DataPulseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Process?> GetAsync(int id)
        {
            return await _dbContext.Processes.AsNoTracking().FirstOrDefaultAsync(p => p.ProcessId == id);
        }

        public async Task<IReadOnlyCollection<Process>> GetByTaskAsync(int taskId)
        {
            return await _dbContext.Processes.Where(p => p.TaskId == taskId).OrderBy(p => p.ExecutionOrder).AsNoTracking().ToListAsync();
        }

        public async Task UpdateStatusAsync(int processId, string status, string? error = null, string? user = null)
        {
            var process = await _dbContext.Processes.FirstOrDefaultAsync(p => p.ProcessId == processId);
            if (process == null)
            {
                return;
            }

            process.Status = status;
            process.LastErrorMessage = error;
            process.LastRunBy = user;
            await _dbContext.SaveChangesAsync();
        }

        public async Task RecordRunTimesAsync(int processId, DateTime start, DateTime? end)
        {
            var process = await _dbContext.Processes.FirstOrDefaultAsync(p => p.ProcessId == processId);
            if (process == null)
            {
                return;
            }

            process.StartTime = start;
            process.EndTime = end;
            await _dbContext.SaveChangesAsync();
        }
    }
}
