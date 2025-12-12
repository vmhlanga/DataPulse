using System.Linq;
using System.Threading.Tasks;
using DataPulse.Application.Services;
using DataPulse.Web.Models;
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

        [HttpGet("servers")]
        public async Task<IActionResult> Servers()
        {
            var viewModel = await BuildCatalogViewModel();
            return View(viewModel);
        }

        [HttpPost("servers/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddServer(NewAgentInput newAgent)
        {
            if (!ModelState.IsValid)
            {
                var invalidModel = await BuildCatalogViewModel();
                invalidModel.NewAgent = newAgent;
                return View("Servers", invalidModel);
            }

            await _processCatalogService.AddAgentAsync(newAgent.AgentName);
            return RedirectToAction(nameof(Servers));
        }

        [HttpPost("servers/jobs/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddJob(NewJobInput newJob)
        {
            if (!ModelState.IsValid)
            {
                var invalidModel = await BuildCatalogViewModel();
                invalidModel.NewJob = newJob;
                return View("Servers", invalidModel);
            }

            var request = new ProcessCatalogCreateRequest
            {
                ProcessName = newJob.ProcessName,
                ProcessDescription = newJob.ProcessDescription,
                ProcessTypeId = newJob.ProcessTypeId,
                AgentMasterId = newJob.AgentMasterId,
                PrimaryStep = new StepCatalogCreateRequest
                {
                    StepName = newJob.StepName,
                    StepDescription = newJob.StepDescription,
                    ServerName = newJob.ServerName,
                    DatabaseName = newJob.DatabaseName,
                    SpName = newJob.SpName,
                    SsisPackageName = newJob.SsisPackageName,
                    SsisSolutionName = newJob.SsisSolutionName,
                    ExecutePath = newJob.ExecutePath
                }
            };

            await _processCatalogService.AddProcessAsync(request);
            return RedirectToAction(nameof(Servers));
        }

        private async Task<CatalogAdminViewModel> BuildCatalogViewModel()
        {
            var agents = await _processCatalogService.GetAgentsAsync();
            var processes = await _processCatalogService.GetProcessesAsync();

            return new CatalogAdminViewModel
            {
                Agents = agents,
                Processes = processes,
                NewJob = new NewJobInput
                {
                    AgentMasterId = agents.FirstOrDefault()?.AgentMasterId ?? 0
                }
            };
        }
    }
}
