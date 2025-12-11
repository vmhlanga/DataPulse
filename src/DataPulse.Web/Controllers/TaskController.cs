using System.Threading.Tasks;
using DataPulse.Application.Orchestration;
using DataPulse.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataPulse.Web.Controllers
{
    [Route("tasks")]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly TaskOrchestrator _orchestrator;

        public TaskController(ITaskService taskService, TaskOrchestrator orchestrator)
        {
            _taskService = taskService;
            _orchestrator = orchestrator;
        }

        [HttpGet("")]
        [HttpGet("/")]
        public async Task<IActionResult> Index()
        {
            var tasks = await _taskService.GetRecentAsync();
            return View(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var task = await _taskService.GetAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        [HttpPost("{id}/run")]
        public async Task<IActionResult> RunTask(int id)
        {
            await _orchestrator.OrchestrateTaskAsync(id, User.Identity?.Name ?? "anonymous");
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
