using System.Threading.Tasks;
using DataPulse.Application.Orchestration;
using DataPulse.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataPulse.Web.Controllers
{
    [Route("tasks")]
    public class TaskController : Controller
    {
        private readonly TaskOrchestrator _orchestrator;
        private readonly IProcessCatalogService _processCatalogService;

        public TaskController(IProcessCatalogService processCatalogService, TaskOrchestrator orchestrator)
        {
            _processCatalogService = processCatalogService;
            _orchestrator = orchestrator;
        }

        [HttpGet("")]
        [HttpGet("/")]
        public async Task<IActionResult> Index()
        {
            var processes = await _processCatalogService.GetProcessesAsync();
            return View(processes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var process = await _processCatalogService.GetProcessAsync(id);
            if (process == null)
            {
                return NotFound();
            }

            return View(process);
        }

        [HttpPost("{id}/run")]
        public async Task<IActionResult> RunTask(int id)
        {
            await _orchestrator.OrchestrateTaskAsync(id, User.Identity?.Name ?? "anonymous");
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
