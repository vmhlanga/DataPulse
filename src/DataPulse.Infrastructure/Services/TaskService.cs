using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPulse.Application.Services;
using DataPulse.Domain.Models;
using DataPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TaskStatus = DataPulse.Domain.Enums.TaskStatus;

namespace DataPulse.Infrastructure.Services
{
    public class TaskService : ITaskService
    {
        private readonly DataPulseDbContext _dbContext;

        public TaskService(DataPulseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DataTask?> GetAsync(int id)
        {
            return await _dbContext.Tasks.Include(t => t.Processes.OrderBy(p => p.ExecutionOrder)).FirstOrDefaultAsync(t => t.TaskId == id);
        }

        public async Task<IReadOnlyCollection<DataTask>> GetRecentAsync(int take = 20)
        {
            return await _dbContext.Tasks.OrderByDescending(t => t.LastRunStartTime).Take(take).AsNoTracking().ToListAsync();
        }

        public async Task<int> CreateAsync(DataTask task)
        {
            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();
            return task.TaskId;
        }

        public async Task UpdateStatusAsync(int taskId, string status, string? user)
        {
            var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId);
            if (task == null)
            {
                return;
            }

            if (Enum.TryParse<TaskStatus>(status, out var parsed))
            {
                task.Status = parsed;
            }
            task.LastRunBy = user;
            await _dbContext.SaveChangesAsync();
        }

        public async Task RecordRunTimesAsync(int taskId, DateTime start, DateTime? end)
        {
            var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId);
            if (task == null)
            {
                return;
            }

            task.LastRunStartTime = start;
            task.LastRunEndTime = end;
            await _dbContext.SaveChangesAsync();
        }
    }
}
