using System.ComponentModel.DataAnnotations;

namespace DataPulse.Application.Services
{
    public class ProcessCatalogCreateRequest
    {
        [Required]
        public string ProcessName { get; set; } = string.Empty;

        public string? ProcessDescription { get; set; }

        [Range(1, int.MaxValue)]
        public int ProcessTypeId { get; set; }

        [Range(1, long.MaxValue)]
        public long AgentMasterId { get; set; }

        [Required]
        public StepCatalogCreateRequest PrimaryStep { get; set; } = new();
    }

    public class StepCatalogCreateRequest
    {
        [Required]
        public string StepName { get; set; } = string.Empty;

        public string? StepDescription { get; set; }

        public string? ServerName { get; set; }

        public string? DatabaseName { get; set; }

        public string? SpName { get; set; }

        public string? SsisPackageName { get; set; }

        public string? SsisSolutionName { get; set; }

        public string? ExecutePath { get; set; }
    }
}
