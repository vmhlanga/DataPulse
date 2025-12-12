using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPulse.Application.Services;
using DataPulse.Domain.Models;
using DataPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataPulse.Infrastructure.Services
{
    public class ProcessCatalogService : IProcessCatalogService
    {
        private readonly DataPulseDbContext _dbContext;

        public ProcessCatalogService(DataPulseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyCollection<ProcessMaster>> GetProcessesAsync()
        {
            var processes = await _dbContext.ProcessCatalog
                .Include(p => p.Steps)
                .AsNoTracking()
                .ToListAsync();

            foreach (var process in processes)
            {
                process.Steps = process.Steps.OrderBy(s => s.StepId).ToList();
            }

            return processes;
        }

        public async Task<ProcessMaster?> GetProcessAsync(int processId)
        {
            var process = await _dbContext.ProcessCatalog
                .Include(p => p.Steps)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProcessId == processId);

            if (process != null)
            {
                process.Steps = process.Steps.OrderBy(s => s.StepId).ToList();
            }

            return process;
        }

        public async Task<IReadOnlyCollection<StepMaster>> GetStepsAsync(int processId)
        {
            return await _dbContext.StepCatalog
                .Where(s => s.ProcessId == processId)
                .OrderBy(s => s.StepId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<StepMaster?> GetStepAsync(int stepId)
        {
            return await _dbContext.StepCatalog.AsNoTracking().FirstOrDefaultAsync(s => s.StepId == stepId);
        }
    }
}
