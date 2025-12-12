using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPulse.Application.Execution;
using DataPulse.Application.Orchestration;
using DataPulse.Application.Services;
using DataPulse.Domain.Enums;
using DataPulse.Domain.Models;
using Xunit;

namespace DataPulse.Tests
{
    public class TaskOrchestratorTests
    {
        [Fact]
        public async Task OrchestrateTaskAsync_Should_Mark_Task_Success_When_All_Steps_Succeed()
        {
            var catalogService = new InMemoryCatalogService();

            var dispatcher = new StubDispatcher(success: true);
            var orchestrator = new TaskOrchestrator(catalogService, dispatcher);

            await orchestrator.OrchestrateTaskAsync(catalogService.Process.ProcessId, "tester");

            Assert.Equal(2, dispatcher.Executions.Count);
        }

        [Fact]
        public async Task OrchestrateTaskAsync_Should_Stop_On_Failure_When_Not_Continuing()
        {
            var catalogService = new InMemoryCatalogService();

            var dispatcher = new StubDispatcher(success: false);
            var orchestrator = new TaskOrchestrator(catalogService, dispatcher);

            await orchestrator.OrchestrateTaskAsync(catalogService.Process.ProcessId, "tester");

            Assert.Single(dispatcher.Executions);
        }

        [Fact]
        public async Task OrchestrateTaskAsync_Should_Continue_On_Error_When_Requested()
        {
            var catalogService = new InMemoryCatalogService();

            var dispatcher = new StubDispatcher(success: false);
            var orchestrator = new TaskOrchestrator(catalogService, dispatcher);

            await orchestrator.OrchestrateTaskAsync(catalogService.Process.ProcessId, "tester", continueOnError: true);

            Assert.Equal(2, dispatcher.Executions.Count);
        }

        private class InMemoryCatalogService : IProcessCatalogService
        {
            public ProcessMaster Process { get; } = new()
            {
                ProcessId = 101,
                ProcessName = "Sample Process",
                ProcessTypeId = (int)ProcessType.StoredProcedure,
                Steps = new List<StepMaster>
                {
                    new()
                    {
                        StepId = 1,
                        StepName = "Step A",
                        ProcessId = 101,
                        SpName = "dbo.First",
                        ServerName = "sql-new",
                        DatabaseName = "Launch"
                    },
                    new()
                    {
                        StepId = 2,
                        StepName = "Step B",
                        ProcessId = 101,
                        SpName = "dbo.Second",
                        ServerName = "sql-new",
                        DatabaseName = "Launch"
                    }
                }
            };

            public Task<ProcessMaster?> GetProcessAsync(int processId)
            {
                return Task.FromResult<ProcessMaster?>(processId == Process.ProcessId ? Process : null);
            }

            public Task<IReadOnlyCollection<ProcessMaster>> GetProcessesAsync()
            {
                return Task.FromResult<IReadOnlyCollection<ProcessMaster>>(new List<ProcessMaster> { Process });
            }

            public Task<IReadOnlyCollection<StepMaster>> GetStepsAsync(int processId)
            {
                return Task.FromResult<IReadOnlyCollection<StepMaster>>(Process.Steps.ToList());
            }

            public Task<StepMaster?> GetStepAsync(int stepId)
            {
                return Task.FromResult(Process.Steps.FirstOrDefault(s => s.StepId == stepId));
            }
        }

        private class StubDispatcher : IExecutionDispatcher
        {
            private readonly bool _success;
            public List<(StepMaster Step, ProcessMaster Process)> Executions { get; } = new();

            public StubDispatcher(bool success)
            {
                _success = success;
            }

            public Task<ExecutionResult> ExecuteAsync(StepMaster step, ProcessMaster process, string? runBy)
            {
                Executions.Add((step, process));
                return Task.FromResult(_success
                    ? ExecutionResult.Completed("ok", DateTime.UtcNow)
                    : ExecutionResult.Failed("boom", DateTime.UtcNow));
            }
        }
    }
}
