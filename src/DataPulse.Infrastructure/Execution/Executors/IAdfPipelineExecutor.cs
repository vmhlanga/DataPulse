using System.Collections.Generic;
using System.Threading.Tasks;
using DataPulse.Application.Execution;

namespace DataPulse.Infrastructure.Execution.Executors
{
    public interface IAdfPipelineExecutor
    {
        Task<ExecutionResult> ExecuteAsync(string pipelineName, IDictionary<string, object?> parameters);
    }
}
