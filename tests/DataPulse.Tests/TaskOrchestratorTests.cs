using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPulse.Application.Execution;
using DataPulse.Application.Orchestration;
using DataPulse.Application.Services;
using DataPulse.Domain.Enums;
using TaskStatus = DataPulse.Domain.Enums.TaskStatus;
using DataPulse.Domain.Models;
using Xunit;

namespace DataPulse.Tests
{
    public class TaskOrchestratorTests
    {
        [Fact]
        public async Task OrchestrateTaskAsync_Should_Mark_Task_Success_When_All_Steps_Succeed()
        {
            var taskService = new InMemoryTaskService();
            var processService = new InMemoryProcessService(new List<Process>
            {
                new()
                {
                    ProcessId = 1,
                    TaskId = taskService.CurrentTask.TaskId,
                    ProcessName = "Step A",
                    ExecutionOrder = 1,
                    ProcessType = ProcessType.StoredProcedure,
                    IsActive = true
                },
                new()
                {
                    ProcessId = 2,
                    TaskId = taskService.CurrentTask.TaskId,
                    ProcessName = "Step B",
                    ExecutionOrder = 2,
                    ProcessType = ProcessType.StoredProcedure,
                    IsActive = true
                }
            });

            var dispatcher = new StubDispatcher(success: true);
            var orchestrator = new TaskOrchestrator(taskService, processService, dispatcher);

            await orchestrator.OrchestrateTaskAsync(taskService.CurrentTask.TaskId, "tester");

            Assert.Equal(TaskStatus.Success.ToString(), taskService.StatusHistory.Last());
            Assert.All(processService.ProcessStatuses.Values, status => Assert.Equal(TaskStatus.Success.ToString(), status));
            Assert.All(processService.StartTimes.Values, time => Assert.NotNull(time));
            Assert.All(processService.EndTimes.Values, time => Assert.NotNull(time));
        }

        [Fact]
        public async Task OrchestrateTaskAsync_Should_Stop_On_Failure_When_Not_Continuing()
        {
            var taskService = new InMemoryTaskService();
            var processService = new InMemoryProcessService(new List<Process>
            {
                new()
                {
                    ProcessId = 1,
                    TaskId = taskService.CurrentTask.TaskId,
                    ProcessName = "Step A",
                    ExecutionOrder = 1,
                    ProcessType = ProcessType.StoredProcedure,
                    IsActive = true
                },
                new()
                {
                    ProcessId = 2,
                    TaskId = taskService.CurrentTask.TaskId,
                    ProcessName = "Step B",
                    ExecutionOrder = 2,
                    ProcessType = ProcessType.StoredProcedure,
                    IsActive = true
                }
            });

            var dispatcher = new StubDispatcher(success: false);
            var orchestrator = new TaskOrchestrator(taskService, processService, dispatcher);

            await orchestrator.OrchestrateTaskAsync(taskService.CurrentTask.TaskId, "tester");

            Assert.Equal(TaskStatus.Failed.ToString(), taskService.StatusHistory.Last());
            Assert.Equal(TaskStatus.Failed.ToString(), processService.ProcessStatuses[1]);
            Assert.False(processService.ProcessStatuses.ContainsKey(2), "Second step should not have executed after a failure.");
        }

        [Fact]
        public async Task OrchestrateTaskAsync_Should_Continue_On_Error_When_Requested()
        {
            var taskService = new InMemoryTaskService();
            var processService = new InMemoryProcessService(new List<Process>
            {
                new()
                {
                    ProcessId = 1,
                    TaskId = taskService.CurrentTask.TaskId,
                    ProcessName = "Step A",
                    ExecutionOrder = 1,
                    ProcessType = ProcessType.StoredProcedure,
                    IsActive = true
                },
                new()
                {
                    ProcessId = 2,
                    TaskId = taskService.CurrentTask.TaskId,
                    ProcessName = "Step B",
                    ExecutionOrder = 2,
                    ProcessType = ProcessType.StoredProcedure,
                    IsActive = true
                }
            });

            var dispatcher = new StubDispatcher(success: false);
            var orchestrator = new TaskOrchestrator(taskService, processService, dispatcher);

            await orchestrator.OrchestrateTaskAsync(taskService.CurrentTask.TaskId, "tester", continueOnError: true);

            Assert.Contains(TaskStatus.Success.ToString(), taskService.StatusHistory);
            Assert.Equal(TaskStatus.Failed.ToString(), processService.ProcessStatuses[1]);
            Assert.Equal(TaskStatus.Failed.ToString(), processService.ProcessStatuses[2]);
        }

        private class InMemoryTaskService : ITaskService
        {
            public DataTask CurrentTask { get; } = new()
            {
                TaskId = 42,
                TaskName = "Sample",
                CreatedBy = "tester",
                CreatedOn = DateTime.UtcNow,
                Status = TaskStatus.NotStarted
            };

            public List<string> StatusHistory { get; } = new();

            public Task<int> CreateAsync(DataTask task)
            {
                throw new NotImplementedException();
            }

            public Task<DataTask?> GetAsync(int id)
            {
                return Task.FromResult<DataTask?>(CurrentTask.TaskId == id ? CurrentTask : null);
            }

            public Task<IReadOnlyCollection<DataTask>> GetRecentAsync(int take = 20)
            {
                return Task.FromResult<IReadOnlyCollection<DataTask>>(new List<DataTask> { CurrentTask });
            }

            public Task RecordRunTimesAsync(int taskId, DateTime start, DateTime? end)
            {
                CurrentTask.LastRunStartTime = start;
                CurrentTask.LastRunEndTime = end;
                return Task.CompletedTask;
            }

            public Task UpdateStatusAsync(int taskId, string status, string? user)
            {
                CurrentTask.Status = Enum.TryParse<TaskStatus>(status, out var parsed) ? parsed : CurrentTask.Status;
                StatusHistory.Add(status);
                CurrentTask.LastRunBy = user;
                return Task.CompletedTask;
            }
        }

        private class InMemoryProcessService : IProcessService
        {
            private readonly IReadOnlyCollection<Process> _processes;

            public Dictionary<int, string> ProcessStatuses { get; } = new();
            public Dictionary<int, DateTime?> StartTimes { get; } = new();
            public Dictionary<int, DateTime?> EndTimes { get; } = new();

            public InMemoryProcessService(IReadOnlyCollection<Process> processes)
            {
                _processes = processes;
            }

            public Task<Process?> GetAsync(int id)
            {
                return Task.FromResult(_processes.FirstOrDefault(p => p.ProcessId == id));
            }

            public Task<IReadOnlyCollection<Process>> GetByTaskAsync(int taskId)
            {
                return Task.FromResult(_processes);
            }

            public Task RecordRunTimesAsync(int processId, DateTime start, DateTime? end)
            {
                StartTimes[processId] = start;
                EndTimes[processId] = end;
                return Task.CompletedTask;
            }

            public Task UpdateStatusAsync(int processId, string status, string? error = null, string? user = null)
            {
                ProcessStatuses[processId] = status;
                return Task.CompletedTask;
            }
        }

        private class StubDispatcher : IExecutionDispatcher
        {
            private readonly bool _success;

            public StubDispatcher(bool success)
            {
                _success = success;
            }

            public Task<ExecutionResult> ExecuteAsync(Process process, string? runBy)
            {
                return Task.FromResult(_success
                    ? ExecutionResult.Completed("ok", DateTime.UtcNow)
                    : ExecutionResult.Failed("boom", DateTime.UtcNow));
            }
        }
    }
}
