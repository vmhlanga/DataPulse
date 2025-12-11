using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DataPulse.Application.Execution;
using DataPulse.Domain.Enums;
using DataPulse.Domain.Models;
using DataPulse.Infrastructure.Data;
using DataPulse.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace DataPulse.Tests
{
    public class WebAppSmokeTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public WebAppSmokeTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });
        }

        [Fact]
        public async Task Tasks_Index_Should_Load()
        {
            var response = await _client.GetAsync("/tasks");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("DataPulse Tasks", content);
            Assert.Contains("Smoke Task", content);
        }

        [Fact]
        public async Task Task_Details_Should_Render_Process_List()
        {
            var response = await _client.GetAsync("/tasks/1");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Smoke Task", content);
            Assert.Contains("Seeded step", content);
        }

        [Fact]
        public async Task RunTask_Should_Update_Status_To_Success()
        {
            var response = await _client.PostAsync("/tasks/1/run", new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>()));
            response.EnsureSuccessStatusCode();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataPulseDbContext>();
            var task = await db.Tasks.FirstAsync(t => t.TaskId == 1);

            Assert.Equal(TaskStatus.Success, task.Status);
            Assert.NotNull(task.LastRunStartTime);
            Assert.NotNull(task.LastRunEndTime);
            Assert.True(db.Processes.All(p => p.Status == TaskStatus.Success.ToString()));
        }
    }

    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<DataPulseDbContext>));
                services.RemoveAll(typeof(IExecutionDispatcher));

                services.AddDbContext<DataPulseDbContext>(options => options.UseInMemoryDatabase($"WebAppSmoke-{Guid.NewGuid()}").EnableSensitiveDataLogging());
                services.AddSingleton<IExecutionDispatcher, StubDispatcher>();

                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DataPulseDbContext>();
                db.Database.EnsureCreated();
                Seed(db);
            });
        }

        private static void Seed(DataPulseDbContext db)
        {
            var task = new DataTask
            {
                TaskId = 1,
                TaskName = "Smoke Task",
                Description = "Seeded for integration tests",
                Status = TaskStatus.NotStarted,
                CreatedBy = "tests",
                CreatedOn = DateTime.UtcNow
            };

            var process = new Process
            {
                ProcessId = 1,
                TaskId = 1,
                ProcessName = "Seeded step",
                ProcessType = ProcessType.StoredProcedure,
                ExecutionOrder = 1,
                IsActive = true,
                Status = TaskStatus.NotStarted.ToString()
            };

            task.Processes.Add(process);
            db.Tasks.Add(task);
            db.Processes.Add(process);
            db.SaveChanges();
        }

        private class StubDispatcher : IExecutionDispatcher
        {
            public Task<ExecutionResult> ExecuteAsync(Process process, string? runBy)
            {
                return Task.FromResult(ExecutionResult.Completed("OK", DateTime.UtcNow));
            }
        }
    }
}
