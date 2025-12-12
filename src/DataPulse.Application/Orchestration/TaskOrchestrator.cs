using System;
using System.Linq;
using System.Threading.Tasks;
using DataPulse.Application.Execution;
using DataPulse.Application.Services;

namespace DataPulse.Application.Orchestration
{
    public class TaskOrchestrator
    {
        private readonly IProcessCatalogService _processCatalogService;
        private readonly IExecutionDispatcher _dispatcher;

        public TaskOrchestrator(
            IProcessCatalogService processCatalogService,
            IExecutionDispatcher dispatcher)
        {
            _processCatalogService = processCatalogService;
            _dispatcher = dispatcher;
        }

        public async Task OrchestrateTaskAsync(int processId, string? runBy, bool continueOnError = false)
        {
            var process = await _processCatalogService.GetProcessAsync(processId) ?? throw new InvalidOperationException($"Process {processId} not found");
            var steps = await _processCatalogService.GetStepsAsync(processId);

            foreach (var step in steps)
            {
                if (step.DeleteDate != null)
                {
                    continue;
                }

                var result = await _dispatcher.ExecuteAsync(step, process, runBy);

                if (!result.Success && !continueOnError)
                {
                    return;
                }
            }
        }
    }
}
