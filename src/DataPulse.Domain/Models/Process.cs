using System;
using DataPulse.Domain.Enums;

namespace DataPulse.Domain.Models
{
    public class Process
    {
        public int ProcessId { get; set; }
        public int TaskId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public ProcessType ProcessType { get; set; }
        public int ExecutionOrder { get; set; }
        public string TargetIdentifier { get; set; } = string.Empty;
        public string? ParametersJson { get; set; }
        public string Status { get; set; } = "NotStarted";
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? LastErrorMessage { get; set; }
        public string? LastRunBy { get; set; }
        public bool IsActive { get; set; } = true;
        public DataTask? Task { get; set; }
    }
}
