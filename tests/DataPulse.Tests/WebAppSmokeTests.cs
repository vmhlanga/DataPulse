using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using DataPulse.Application.Execution;
using DataPulse.Domain.Enums;
using DataPulse.Domain.Models;
using DataPulse.Infrastructure.Data;
using DataPulse.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using DomainTaskStatus = DataPulse.Domain.Enums.TaskStatus;

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
        public async Task Process_Run_Should_Record_Status_And_Timestamps()
        {
            var response = await _client.PostAsync("/processes/1/run", new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>()));

            response.EnsureSuccessStatusCode();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataPulseDbContext>();
            var process = await db.Processes.FirstAsync(p => p.ProcessId == 1);

            Assert.Equal(DomainTaskStatus.Success.ToString(), process.Status);
            Assert.NotNull(process.StartTime);
            Assert.NotNull(process.EndTime);
        }

        [Fact]
        public async Task Navigation_Should_Reach_All_User_Facing_Pages()
        {
            var indexResponse = await _client.GetAsync("/tasks");
            indexResponse.EnsureSuccessStatusCode();
            var indexContent = await indexResponse.Content.ReadAsStringAsync();
            Assert.Contains("DataPulse Tasks", indexContent);

            var detailsResponse = await _client.GetAsync("/tasks/1");
            detailsResponse.EnsureSuccessStatusCode();
            var detailsContent = await detailsResponse.Content.ReadAsStringAsync();
            Assert.Contains("Seeded step", detailsContent);

            var processRunResponse = await _client.PostAsync("/processes/1/run", new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>()));
            processRunResponse.EnsureSuccessStatusCode();
            var afterRunContent = await processRunResponse.Content.ReadAsStringAsync();
            Assert.Contains("Smoke Task", afterRunContent);

            var adminResponse = await _client.GetAsync("/admin/tasks");
            adminResponse.EnsureSuccessStatusCode();
            var adminContent = await adminResponse.Content.ReadAsStringAsync();
            Assert.Contains("Admin: Tasks", adminContent);
            Assert.Contains("Smoke Task", adminContent);
        }

        [Fact]
        public async Task RunTask_Should_Update_Status_To_Success()
        {
            var response = await _client.PostAsync("/tasks/1/run", new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>()));
            response.EnsureSuccessStatusCode();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataPulseDbContext>();
            var task = await db.Tasks.FirstAsync(t => t.TaskId == 1);

            Assert.Equal(DomainTaskStatus.Success, task.Status);
            Assert.NotNull(task.LastRunStartTime);
            Assert.NotNull(task.LastRunEndTime);
            Assert.True(db.Processes.All(p => p.Status == DomainTaskStatus.Success.ToString()));
        }
    }

    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"WebAppSmoke-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<DataPulseDbContext>));
                services.RemoveAll(typeof(IExecutionDispatcher));

                services.AddDbContext<DataPulseDbContext>(options => options.UseInMemoryDatabase(_databaseName).EnableSensitiveDataLogging());
                services.AddSingleton<IExecutionDispatcher, StubDispatcher>();
                services.AddSingleton<IStartupFilter, FakeAdminUserStartupFilter>();

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
                Status = DomainTaskStatus.NotStarted,
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
                Status = DomainTaskStatus.NotStarted.ToString()
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

        private class FakeAdminUserStartupFilter : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return app =>
                {
                    app.Use(async (context, pipeline) =>
                    {
                        var claims = new[]
                        {
                            new Claim(ClaimTypes.Name, "integration-admin"),
                            new Claim(ClaimTypes.Role, "Admin"),
                        };

                        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
                        await pipeline();
                    });

                    next(app);
                };
            }
        }
    }
}
