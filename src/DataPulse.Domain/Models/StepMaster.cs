using System;

namespace DataPulse.Domain.Models
{
    public class StepMaster
    {
        public int StepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string StepDescription { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string SpName { get; set; } = string.Empty;
        public string SsisPackageName { get; set; } = string.Empty;
        public string SsisSolutionName { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public byte IsLastStep { get; set; }
        public int DependentStepId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string? ExecutePath { get; set; }

        public ProcessMaster? Process { get; set; }
    }
}
