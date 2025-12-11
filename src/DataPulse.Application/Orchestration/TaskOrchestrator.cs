using System;
using System.Linq;
using System.Threading.Tasks;
using DataPulse.Application.Execution;
using DataPulse.Application.Services;
using DataPulse.Domain.Enums;

namespace DataPulse.Application.Orchestration
{
    public class TaskOrchestrator
    {
        private readonly ITaskService _taskService;
        private readonly IProcessService _processService;
        private readonly IExecutionDispatcher _dispatcher;

        public TaskOrchestrator(
            ITaskService taskService,
            IProcessService processService,
            IExecutionDispatcher dispatcher)
        {
            _taskService = taskService;
            _processService = processService;
            _dispatcher = dispatcher;
        }

        public async Task OrchestrateTaskAsync(int taskId, string? runBy, bool continueOnError = false)
        {
            var task = await _taskService.GetAsync(taskId) ?? throw new InvalidOperationException($"Task {taskId} not found");
            var processes = await _processService.GetByTaskAsync(taskId);

            await _taskService.UpdateStatusAsync(taskId, TaskStatus.InProgress.ToString(), runBy);
            var start = DateTime.UtcNow;
            await _taskService.RecordRunTimesAsync(taskId, start, null);

            foreach (var process in processes.OrderBy(p => p.ExecutionOrder))
            {
                if (!process.IsActive)
                {
                    continue;
                }

                var processStart = DateTime.UtcNow;
                await _processService.UpdateStatusAsync(process.ProcessId, TaskStatus.InProgress.ToString(), null, runBy);
                await _processService.RecordRunTimesAsync(process.ProcessId, processStart, null);

                var result = await _dispatcher.ExecuteAsync(process, runBy);
                await _processService.UpdateStatusAsync(process.ProcessId, result.Success ? TaskStatus.Success.ToString() : TaskStatus.Failed.ToString(), result.Error, runBy);
                await _processService.RecordRunTimesAsync(process.ProcessId, processStart, DateTime.UtcNow);

                if (!result.Success && !continueOnError)
                {
                    await _taskService.UpdateStatusAsync(taskId, TaskStatus.Failed.ToString(), runBy);
                    await _taskService.RecordRunTimesAsync(taskId, start, DateTime.UtcNow);
                    return;
                }
            }

            await _taskService.UpdateStatusAsync(taskId, TaskStatus.Success.ToString(), runBy);
            await _taskService.RecordRunTimesAsync(taskId, start, DateTime.UtcNow);
        }
    }
}
