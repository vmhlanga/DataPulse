using System.Threading.Tasks;
using DataPulse.Application.Execution;
using DataPulse.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataPulse.Web.Controllers
{
    [Route("processes")]
    public class ProcessController : Controller
    {
        private readonly IExecutionDispatcher _dispatcher;
        private readonly IProcessCatalogService _processCatalogService;

        public ProcessController(IProcessCatalogService processCatalogService, IExecutionDispatcher dispatcher)
        {
            _processCatalogService = processCatalogService;
            _dispatcher = dispatcher;
        }

        [HttpPost("{id}/run")]
        public async Task<IActionResult> Run(int id)
        {
            var step = await _processCatalogService.GetStepAsync(id);
            if (step == null)
            {
                return NotFound();
            }

            var process = await _processCatalogService.GetProcessAsync(step.ProcessId);
            if (process == null)
            {
                return NotFound();
            }

            var result = await _dispatcher.ExecuteAsync(step, process, User.Identity?.Name ?? "anonymous");

            TempData["LastExecution"] = result.Success ? "Process completed" : $"Process failed: {result.Error}";
            return RedirectToAction("Details", "Task", new { id = process.ProcessId });
        }
    }
}
