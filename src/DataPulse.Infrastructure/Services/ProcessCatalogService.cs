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
                .Include(p => p.Agent)
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
                .Include(p => p.Agent)
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

        public async Task<IReadOnlyCollection<AgentMaster>> GetAgentsAsync()
        {
            return await _dbContext.Agents
                .OrderBy(a => a.AgentMasterName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<AgentMaster> AddAgentAsync(string agentName)
        {
            var nextAgentId = _dbContext.Agents.Any()
                ? await _dbContext.Agents.MaxAsync(a => a.AgentMasterId) + 1
                : 1;

            var agent = new AgentMaster
            {
                AgentMasterId = nextAgentId,
                AgentMasterName = agentName,
                CreateDate = System.DateTime.UtcNow
            };

            _dbContext.Agents.Add(agent);
            await _dbContext.SaveChangesAsync();

            return agent;
        }

        public async Task<ProcessMaster> AddProcessAsync(ProcessCatalogCreateRequest request)
        {
            var nextProcessId = _dbContext.ProcessCatalog.Any()
                ? await _dbContext.ProcessCatalog.MaxAsync(p => p.ProcessId) + 1
                : 1;

            var nextStepId = _dbContext.StepCatalog.Any()
                ? await _dbContext.StepCatalog.MaxAsync(s => s.StepId) + 1
                : 1;

            var process = new ProcessMaster
            {
                ProcessId = nextProcessId,
                ProcessName = request.ProcessName,
                ProcessDescription = request.ProcessDescription,
                ProcessTypeId = request.ProcessTypeId,
                AgentMasterId = request.AgentMasterId,
                CreateDate = System.DateTime.UtcNow
            };

            var creationDate = process.CreateDate ?? System.DateTime.UtcNow;

            var step = new StepMaster
            {
                StepId = nextStepId,
                StepName = request.PrimaryStep.StepName,
                StepDescription = request.PrimaryStep.StepDescription ?? string.Empty,
                ServerName = request.PrimaryStep.ServerName ?? string.Empty,
                DatabaseName = request.PrimaryStep.DatabaseName ?? string.Empty,
                SpName = request.PrimaryStep.SpName ?? string.Empty,
                SsisPackageName = request.PrimaryStep.SsisPackageName ?? string.Empty,
                SsisSolutionName = request.PrimaryStep.SsisSolutionName ?? string.Empty,
                ProcessId = process.ProcessId,
                IsLastStep = 1,
                DependentStepId = 0,
                CreateDate = creationDate,
                ExecutePath = request.PrimaryStep.ExecutePath
            };

            process.Steps = new List<StepMaster> { step };

            _dbContext.ProcessCatalog.Add(process);
            _dbContext.StepCatalog.Add(step);

            await _dbContext.SaveChangesAsync();

            process.Steps = new List<StepMaster> { step };
            return process;
        }
    }
}
