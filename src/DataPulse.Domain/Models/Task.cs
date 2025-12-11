using System;
using System.Collections.Generic;
using DataPulse.Domain.Enums;

namespace DataPulse.Domain.Models
{
    public class DataTask
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime? LastRunStartTime { get; set; }
        public DateTime? LastRunEndTime { get; set; }
        public string? LastRunBy { get; set; }
        public ICollection<Process> Processes { get; set; } = new List<Process>();
    }
}
