using System.Collections.Generic;
using System.Threading.Tasks;
using DataPulse.Domain.Models;

namespace DataPulse.Application.Services
{
    public interface IProcessCatalogService
    {
        Task<IReadOnlyCollection<ProcessMaster>> GetProcessesAsync();
        Task<ProcessMaster?> GetProcessAsync(int processId);
        Task<IReadOnlyCollection<StepMaster>> GetStepsAsync(int processId);
        Task<StepMaster?> GetStepAsync(int stepId);
        Task<IReadOnlyCollection<AgentMaster>> GetAgentsAsync();
        Task<AgentMaster> AddAgentAsync(string agentName);
        Task<ProcessMaster> AddProcessAsync(ProcessCatalogCreateRequest request);
    }
}
