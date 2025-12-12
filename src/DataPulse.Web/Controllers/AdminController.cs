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
        private readonly IProcessCatalogService _processCatalogService;

        public AdminController(IProcessCatalogService processCatalogService)
        {
            _processCatalogService = processCatalogService;
        }

        [HttpGet("tasks")]
        public async Task<IActionResult> Tasks()
        {
            var processes = await _processCatalogService.GetProcessesAsync();
            return View(processes);
        }
    }
}
