using System.Threading.Tasks;
using DataPulse.Domain.Models;

namespace DataPulse.Application.Execution
{
    public interface IExecutionDispatcher
    {
        Task<ExecutionResult> ExecuteAsync(Process process, string? runBy);
    }
}
