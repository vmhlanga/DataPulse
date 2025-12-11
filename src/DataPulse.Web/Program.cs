using DataPulse.Application.Execution;
using DataPulse.Application.Orchestration;
using DataPulse.Application.Services;
using DataPulse.Infrastructure.Data;
using DataPulse.Infrastructure.Execution;
using DataPulse.Infrastructure.Execution.Executors;
using DataPulse.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DataPulseDbContext>(options => options.UseInMemoryDatabase("DataPulse"));

builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddScoped<IExecutionDispatcher, ExecutionDispatcher>();
builder.Services.AddScoped<TaskOrchestrator>();
builder.Services.AddScoped<ISSISExecutor, SsisCatalogExecutor>();
builder.Services.AddScoped<IStoredProcedureExecutor, StoredProcedureExecutor>();
builder.Services.AddScoped<IAdfPipelineExecutor, AdfPipelineExecutor>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Task}/{action=Index}/{id?}");

app.Run();
