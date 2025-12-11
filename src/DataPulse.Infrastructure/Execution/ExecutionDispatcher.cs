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

        public Task<ExecutionResult> ExecuteAsync(Process process, string? runBy)
        {
            var parameters = string.IsNullOrWhiteSpace(process.ParametersJson)
                ? new Dictionary<string, object?>()
                : JsonSerializer.Deserialize<Dictionary<string, object?>>(process.ParametersJson) ?? new();

            return process.ProcessType switch
            {
                ProcessType.SSIS => _ssisExecutor.ExecuteAsync(process.TargetIdentifier, parameters),
                ProcessType.StoredProcedure => _storedProcedureExecutor.ExecuteAsync(process.TargetIdentifier, parameters),
                ProcessType.ADFPipeline => _adfExecutor.ExecuteAsync(process.TargetIdentifier, parameters),
                _ => Task.FromResult(ExecutionResult.Failed($"Unsupported process type {process.ProcessType}", System.DateTime.UtcNow))
            };
        }
    }
}
