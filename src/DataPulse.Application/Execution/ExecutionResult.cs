using System;

namespace DataPulse.Application.Execution
{
    public class ExecutionResult
    {
        public bool Success { get; init; }
        public string? Output { get; init; }
        public string? Error { get; init; }
        public DateTime StartedAt { get; init; }
        public DateTime EndedAt { get; init; }
        public TimeSpan Duration => EndedAt - StartedAt;

        public static ExecutionResult Completed(string? output, DateTime started)
        {
            return new ExecutionResult
            {
                Success = true,
                Output = output,
                StartedAt = started,
                EndedAt = DateTime.UtcNow
            };
        }

        public static ExecutionResult Failed(string message, DateTime started)
        {
            return new ExecutionResult
            {
                Success = false,
                Error = message,
                StartedAt = started,
                EndedAt = DateTime.UtcNow
            };
        }
    }
}
