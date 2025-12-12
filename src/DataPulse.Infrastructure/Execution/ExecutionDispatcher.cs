using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DataPulse.Application.Execution;
using DataPulse.Domain.Enums;
using DataPulse.Domain.Models;
using DataPulse.Infrastructure.Execution.Executors;

namespace DataPulse.Infrastructure.Execution
{
    public class ExecutionDispatcher : IExecutionDispatcher
    {
        private readonly ISSISExecutor _ssisExecutor;
        private readonly IStoredProcedureExecutor _storedProcedureExecutor;
        private readonly IAdfPipelineExecutor _adfExecutor;

        public ExecutionDispatcher(
            ISSISExecutor ssisExecutor,
            IStoredProcedureExecutor storedProcedureExecutor,
            IAdfPipelineExecutor adfExecutor)
        {
            _ssisExecutor = ssisExecutor;
            _storedProcedureExecutor = storedProcedureExecutor;
            _adfExecutor = adfExecutor;
        }

        public Task<ExecutionResult> ExecuteAsync(StepMaster step, ProcessMaster process, string? runBy)
        {
            var parameters = string.IsNullOrWhiteSpace(process.ProcessDescription)
                ? new Dictionary<string, object?>()
                : JsonSerializer.Deserialize<Dictionary<string, object?>>(process.ProcessDescription) ?? new();

            var processType = (ProcessType)process.ProcessTypeId;

            return processType switch
            {
                ProcessType.SSIS => _ssisExecutor.ExecuteAsync(step.SsisPackageName, parameters, step.ServerName),
                ProcessType.StoredProcedure => _storedProcedureExecutor.ExecuteAsync(step.SpName, parameters, step.ServerName, step.DatabaseName),
                ProcessType.ADFPipeline => _adfExecutor.ExecuteAsync(process.ProcessName, parameters),
                _ => Task.FromResult(ExecutionResult.Failed($"Unsupported process type {process.ProcessTypeId}", System.DateTime.UtcNow))
            };
        }
    }
}
