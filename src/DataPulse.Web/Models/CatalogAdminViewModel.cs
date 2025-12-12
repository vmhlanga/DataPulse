using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataPulse.Application.Services;
using DataPulse.Domain.Models;

namespace DataPulse.Web.Models
{
    public class CatalogAdminViewModel
    {
        public IReadOnlyCollection<AgentMaster> Agents { get; set; } = new List<AgentMaster>();

        public IReadOnlyCollection<ProcessMaster> Processes { get; set; } = new List<ProcessMaster>();

        public NewAgentInput NewAgent { get; set; } = new();

        public NewJobInput NewJob { get; set; } = new();
    }

    public class NewAgentInput
    {
        [Required]
        [StringLength(500)]
        [Display(Name = "Server or Agent Name")]
        public string AgentName { get; set; } = string.Empty;
    }

    public class NewJobInput
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Job Name")]
        public string ProcessName { get; set; } = string.Empty;

        [StringLength(5000)]
        [Display(Name = "Description")]
        public string? ProcessDescription { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Process Type Id")]
        public int ProcessTypeId { get; set; } = 1;

        [Range(1, int.MaxValue)]
        [Display(Name = "Server/Agent")]
        public int AgentMasterId { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "Primary Step Name")]
        public string StepName { get; set; } = string.Empty;

        [StringLength(2000)]
        [Display(Name = "Step Description")]
        public string? StepDescription { get; set; }

        [StringLength(1000)]
        [Display(Name = "Server Name")]
        public string? ServerName { get; set; }

        [StringLength(1000)]
        [Display(Name = "Database Name")]
        public string? DatabaseName { get; set; }

        [StringLength(1000)]
        [Display(Name = "Stored Procedure Name")]
        public string? SpName { get; set; }

        [StringLength(250)]
        [Display(Name = "SSIS Package")]
        public string? SsisPackageName { get; set; }

        [StringLength(250)]
        [Display(Name = "SSIS Solution")]
        public string? SsisSolutionName { get; set; }

        [StringLength(2000)]
        [Display(Name = "Execute Path / Notes")]
        public string? ExecutePath { get; set; }
    }
}
