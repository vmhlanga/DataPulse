using System;
using System.Collections.Generic;

namespace DataPulse.Domain.Models
{
    public class ProcessMaster
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string? ProcessDescription { get; set; }
        public int DependentProcessId { get; set; }
        public int ProcessTypeId { get; set; }
        public long? AgentMasterId { get; set; }
        public string? FromEmailAddress { get; set; }
        public string? SuccessToEmailAddress { get; set; }
        public string? SuccessCcEmailAddress { get; set; }
        public string? FailureToEmailAddress { get; set; }
        public string? FailureCcEmailAddress { get; set; }
        public bool? SendEmailOnSuccess { get; set; }
        public bool? SendEmailOnFailure { get; set; }
        public bool? CanBeRerun { get; set; }
        public bool? HasEventHandler { get; set; }
        public bool? HasExceptions { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public bool IsCritical { get; set; }

        public AgentMaster? Agent { get; set; }
        public ICollection<StepMaster> Steps { get; set; } = new List<StepMaster>();
    }
}
