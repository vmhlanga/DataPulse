# Environment notes for DataPulse orchestration

DataPulse needs to reach multiple SQL Server instances that host the legacy assets. Use these details to configure connection strings, linked servers, and integration account secrets in your deployment pipelines.

## Server layout

- **sql03**: hosts the **Launch** database containing the provided `process.process_master`, `process.step_master`, and `sql_job.agent_master` catalog tables. Point EF Core migrations and metadata queries at this instance.
- **sql02**: hosts the SSIS packages and most SQL Agent jobs referenced by the catalog metadata. SSIS execution and SQL Agent proxy interactions should target this server.

## Deployment guidance

- In GitHub Actions, set secrets such as `SQL03_LAUNCH_CONNECTION` (for EF Core/Task metadata) and `SQL02_SSIS_CONNECTION` (for SSIS/Agent job execution). Use environment-specific variables or environments to scope credentials.
- If you need cross-server execution (e.g., read metadata from sql03 but run SSIS on sql02), ensure firewall rules and linked server permissions allow the orchestrator host to reach both instances.
- Keep mail routing settings (success/failure recipients from the catalog tables) in app configuration so you can adjust notification policies without code changes.

## Local development

For local builds and unit tests, the solution uses an in-memory database. Integration tests that exercise the real servers should be opt-in and require the connection strings above.
