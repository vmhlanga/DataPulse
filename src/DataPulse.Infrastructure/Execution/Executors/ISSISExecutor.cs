using System.Collections.Generic;
using System.Threading.Tasks;
using DataPulse.Application.Execution;

namespace DataPulse.Infrastructure.Execution.Executors
{
    public interface ISSISExecutor
    {
        Task<ExecutionResult> ExecuteAsync(string packageName, IDictionary<string, object?> parameters);
    }
}
