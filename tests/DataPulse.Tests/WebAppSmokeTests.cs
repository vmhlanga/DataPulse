using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using DataPulse.Application.Execution;
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
            Assert.Contains("Process Catalog", content);
            Assert.Contains("Smoke Process", content);
        }

        [Fact]
        public async Task Task_Details_Should_Render_Process_List()
        {
            var response = await _client.GetAsync("/tasks/1");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Smoke Process", content);
            Assert.Contains("Seeded step", content);
        }

        [Fact]
        public async Task Process_Run_Should_Invoke_Dispatcher_For_Step()
        {
            var dispatcher = (StubDispatcher)_factory.Services.GetRequiredService<IExecutionDispatcher>();
            dispatcher.Reset();

            var response = await _client.PostAsync("/processes/1/run", new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>()));

            response.EnsureSuccessStatusCode();

            Assert.Equal(1, dispatcher.ExecutionCount);
            Assert.Equal(1, dispatcher.LastStep?.StepId);
            Assert.Equal(1, dispatcher.LastProcess?.ProcessId);
        }

        [Fact]
        public async Task Navigation_Should_Reach_All_User_Facing_Pages()
        {
            var indexResponse = await _client.GetAsync("/tasks");
            indexResponse.EnsureSuccessStatusCode();
            var indexContent = await indexResponse.Content.ReadAsStringAsync();
            Assert.Contains("Process Catalog", indexContent);

            var detailsResponse = await _client.GetAsync("/tasks/1");
            detailsResponse.EnsureSuccessStatusCode();
            var detailsContent = await detailsResponse.Content.ReadAsStringAsync();
            Assert.Contains("Seeded step", detailsContent);

            var processRunResponse = await _client.PostAsync("/processes/1/run", new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>()));
            processRunResponse.EnsureSuccessStatusCode();
            var afterRunContent = await processRunResponse.Content.ReadAsStringAsync();
            Assert.Contains("Smoke Process", afterRunContent);

            var adminResponse = await _client.GetAsync("/admin/tasks");
            adminResponse.EnsureSuccessStatusCode();
            var adminContent = await adminResponse.Content.ReadAsStringAsync();
            Assert.Contains("Admin: Processes", adminContent);
            Assert.Contains("Smoke Process", adminContent);
        }

        [Fact]
        public async Task RunTask_Should_Invoke_All_Steps()
        {
            var dispatcher = (StubDispatcher)_factory.Services.GetRequiredService<IExecutionDispatcher>();
            dispatcher.Reset();

            var response = await _client.PostAsync("/tasks/1/run", new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>()));
            response.EnsureSuccessStatusCode();

            Assert.Equal(1, dispatcher.ExecutionCount);
            Assert.Equal(1, dispatcher.LastStep?.StepId);
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
            var process = new ProcessMaster
            {
                ProcessId = 1,
                ProcessName = "Smoke Process",
                ProcessDescription = "Seeded for integration tests",
                CreateDate = DateTime.UtcNow
            };

            var step = new StepMaster
            {
                StepId = 1,
                ProcessId = 1,
                StepName = "Seeded step",
                StepDescription = "Seeded catalog step",
                ServerName = "ssis-server",
                DatabaseName = "ssis-db",
                SpName = "dbo.usp_run_me",
                CreateDate = DateTime.UtcNow
            };

            process.Steps.Add(step);
            db.ProcessCatalog.Add(process);
            db.StepCatalog.Add(step);
            db.SaveChanges();
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

    public class StubDispatcher : IExecutionDispatcher
    {
        public int ExecutionCount { get; private set; }
        public StepMaster? LastStep { get; private set; }
        public ProcessMaster? LastProcess { get; private set; }

        public void Reset()
        {
            ExecutionCount = 0;
            LastStep = null;
            LastProcess = null;
        }

        public Task<ExecutionResult> ExecuteAsync(StepMaster step, ProcessMaster process, string? runBy)
        {
            ExecutionCount++;
            LastStep = step;
            LastProcess = process;

            return Task.FromResult(ExecutionResult.Completed("OK", DateTime.UtcNow));
        }
    }
}
