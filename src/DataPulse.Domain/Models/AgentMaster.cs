using System;

namespace DataPulse.Domain.Models
{
    public class AgentMaster
    {
        public long AgentMasterId { get; set; }
        public string? AgentMasterName { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
