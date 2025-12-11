using System.Threading.Tasks;
using DataPulse.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataPulse.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ITaskService _taskService;

        public AdminController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("tasks")]
        public async Task<IActionResult> Tasks()
        {
            var tasks = await _taskService.GetRecentAsync(200);
            return View(tasks);
        }
    }
}
