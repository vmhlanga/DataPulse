using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DataPulse.Application.Execution;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataPulse.Infrastructure.Execution.Executors
{
    public class StoredProcedureExecutor : IStoredProcedureExecutor
    {
        private readonly string _connectionString;
        private readonly ILogger<StoredProcedureExecutor> _logger;

        public StoredProcedureExecutor(IConfiguration configuration, ILogger<StoredProcedureExecutor> logger)
        {
            _connectionString = configuration.GetConnectionString("DataPulseDb") ?? string.Empty;
            _logger = logger;
        }

        public async Task<ExecutionResult> ExecuteAsync(string storedProcedureName, IDictionary<string, object?> parameters, string serverName, string databaseName)
        {
            var started = DateTime.UtcNow;
            try
            {
                var builder = new SqlConnectionStringBuilder(_connectionString)
                {
                    DataSource = serverName,
                    InitialCatalog = databaseName
                };

                await using var connection = new SqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand(storedProcedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                foreach (var kvp in parameters)
                {
                    command.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                }

                var rows = await command.ExecuteNonQueryAsync();
                _logger.LogInformation(
                    "Executed stored procedure {StoredProcedure} on {Server}/{Database} affecting {Rows} rows",
                    storedProcedureName,
                    serverName,
                    databaseName,
                    rows);

                return new ExecutionResult
                {
                    Success = true,
                    Output = $"Rows affected: {rows}",
                    StartedAt = started,
                    EndedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stored procedure {StoredProcedure} failed", storedProcedureName);
                return new ExecutionResult
                {
                    Success = false,
                    Error = ex.Message,
                    StartedAt = started,
                    EndedAt = DateTime.UtcNow
                };
            }
        }
    }
}
