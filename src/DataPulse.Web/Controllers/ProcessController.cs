using System.Threading.Tasks;
using DataPulse.Application.Execution;
using DataPulse.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataPulse.Web.Controllers
{
    [Route("processes")]
    public class ProcessController : Controller
    {
        private readonly IProcessService _processService;
        private readonly IExecutionDispatcher _dispatcher;

        public ProcessController(IProcessService processService, IExecutionDispatcher dispatcher)
        {
            _processService = processService;
            _dispatcher = dispatcher;
        }

        [HttpPost("{id}/run")]
        public async Task<IActionResult> Run(int id)
        {
            var process = await _processService.GetAsync(id);
            if (process == null)
            {
                return NotFound();
            }

            var result = await _dispatcher.ExecuteAsync(process, User.Identity?.Name ?? "anonymous");
            await _processService.UpdateStatusAsync(id, result.Success ? "Success" : "Failed", result.Error, User.Identity?.Name);
            await _processService.RecordRunTimesAsync(id, result.StartedAt, result.EndedAt);

            TempData["LastExecution"] = result.Success ? "Process completed" : $"Process failed: {result.Error}";
            return RedirectToAction("Details", "Task", new { id = process.TaskId });
        }

        [HttpGet("{id}/history")]
        public async Task<IActionResult> History(int id)
        {
            var process = await _processService.GetAsync(id);
            if (process == null)
            {
                return NotFound();
            }

            return View(process);
        }
    }
}
