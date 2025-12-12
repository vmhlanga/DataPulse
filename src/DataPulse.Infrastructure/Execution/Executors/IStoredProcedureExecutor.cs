using System.Collections.Generic;
using System.Threading.Tasks;
using DataPulse.Application.Execution;

namespace DataPulse.Infrastructure.Execution.Executors
{
    public interface IStoredProcedureExecutor
    {
        Task<ExecutionResult> ExecuteAsync(string storedProcedureName, IDictionary<string, object?> parameters, string serverName, string databaseName);
    }
}
