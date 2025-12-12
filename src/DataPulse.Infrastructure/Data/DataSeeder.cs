using System;
using System.Collections.Generic;
using System.Linq;
using DataPulse.Domain.Models;

namespace DataPulse.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static void Seed(DataPulseDbContext context)
        {
            if (context.ProcessCatalog.Any())
            {
                return;
            }

            var utcNow = DateTime.UtcNow;

            var agents = new List<AgentMaster>
            {
                new AgentMaster
                {
                    AgentMasterId = 701,
                    AgentMasterName = "Aurora Control Tower",
                    CreateDate = utcNow.AddDays(-30)
                },
                new AgentMaster
                {
                    AgentMasterId = 702,
                    AgentMasterName = "Nimbus Data Runner",
                    CreateDate = utcNow.AddDays(-21)
                }
            };

            var processes = new List<ProcessMaster>
            {
                new ProcessMaster
                {
                    ProcessId = 1001,
                    ProcessName = "Nightly Warehouse Refresh",
                    ProcessDescription = "Curates telemetry, rebuilds star schemas, and publishes a fresh semantic layer for BI.",
                    ProcessTypeId = 1,
                    AgentMasterId = agents[0].AgentMasterId,
                    SuccessToEmailAddress = "ops@datapulse.test",
                    SendEmailOnSuccess = true,
                    SendEmailOnFailure = true,
                    CreateDate = utcNow.AddDays(-10),
                    Steps = new List<StepMaster>
                    {
                        new StepMaster
                        {
                            StepId = 201,
                            StepName = "Validate upstream feeds",
                            StepDescription = "Ensure raw sensor files landed and schema drift rules pass",
                            ServerName = "sql-west-01",
                            DatabaseName = "ingest_ops",
                            SpName = "health.usp_validate_feeds",
                            SsisPackageName = "",
                            SsisSolutionName = "",
                            ProcessId = 1001,
                            IsLastStep = 0,
                            DependentStepId = 0,
                            CreateDate = utcNow.AddDays(-10),
                            ExecutePath = "health/validate-feeds"
                        },
                        new StepMaster
                        {
                            StepId = 202,
                            StepName = "Build dimensional layer",
                            StepDescription = "Run SSIS solution to rebuild slowly changing dimensions",
                            ServerName = "ssis-farm-02",
                            DatabaseName = "warehouse",
                            SpName = "",
                            SsisPackageName = "RebuildDimensions.dtsx",
                            SsisSolutionName = "WarehouseBuild.sln",
                            ProcessId = 1001,
                            IsLastStep = 0,
                            DependentStepId = 201,
                            CreateDate = utcNow.AddDays(-10),
                            ExecutePath = "ssis/warehouse-dimensions"
                        },
                        new StepMaster
                        {
                            StepId = 203,
                            StepName = "Publish semantic model",
                            StepDescription = "Execute SP to snapshot curated marts and push tabular refresh",
                            ServerName = "sql-west-analytics",
                            DatabaseName = "semantic_marts",
                            SpName = "orchestration.usp_publish_semantic_layer",
                            SsisPackageName = "",
                            SsisSolutionName = "",
                            ProcessId = 1001,
                            IsLastStep = 1,
                            DependentStepId = 202,
                            CreateDate = utcNow.AddDays(-10),
                            ExecutePath = "sql/publish-semantic"
                        }
                    }
                },
                new ProcessMaster
                {
                    ProcessId = 1002,
                    ProcessName = "Customer 360 Sync",
                    ProcessDescription = "Blends CRM, billing, and product signals into a single customer profile score each afternoon.",
                    ProcessTypeId = 2,
                    AgentMasterId = agents[1].AgentMasterId,
                    FailureToEmailAddress = "revops@datapulse.test",
                    SendEmailOnFailure = true,
                    CanBeRerun = true,
                    CreateDate = utcNow.AddDays(-8),
                    Steps = new List<StepMaster>
                    {
                        new StepMaster
                        {
                            StepId = 301,
                            StepName = "Ingest CRM delta",
                            StepDescription = "Pull Salesforce delta set and land to staging",
                            ServerName = "sql-east-02",
                            DatabaseName = "stg_crm",
                            SpName = "ingest.usp_pull_salesforce_delta",
                            ProcessId = 1002,
                            IsLastStep = 0,
                            DependentStepId = 0,
                            CreateDate = utcNow.AddDays(-8),
                            ExecutePath = "crm/delta-import"
                        },
                        new StepMaster
                        {
                            StepId = 302,
                            StepName = "Score customer health",
                            StepDescription = "Run Python scoring pipeline via ADF to merge usage and support signals",
                            ServerName = "adf-east",
                            DatabaseName = "",
                            SpName = "",
                            SsisPackageName = "",
                            SsisSolutionName = "",
                            ProcessId = 1002,
                            IsLastStep = 0,
                            DependentStepId = 301,
                            CreateDate = utcNow.AddDays(-8),
                            ExecutePath = "adf/pipelines/customer-health"
                        },
                        new StepMaster
                        {
                            StepId = 303,
                            StepName = "Push scores to apps",
                            StepDescription = "Publish refreshed scores to feature store and notify product webhooks",
                            ServerName = "sql-east-02",
                            DatabaseName = "feature_store",
                            SpName = "sync.usp_publish_health_scores",
                            ProcessId = 1002,
                            IsLastStep = 1,
                            DependentStepId = 302,
                            CreateDate = utcNow.AddDays(-8),
                            ExecutePath = "sql/push-scores"
                        }
                    }
                },
                new ProcessMaster
                {
                    ProcessId = 1003,
                    ProcessName = "Marketing Attribution Sandbox",
                    ProcessDescription = "Ad-hoc sandbox that marketers can re-run to test new attribution rules before production.",
                    ProcessTypeId = 3,
                    AgentMasterId = agents[0].AgentMasterId,
                    HasEventHandler = true,
                    CanBeRerun = true,
                    CreateDate = utcNow.AddDays(-5),
                    Steps = new List<StepMaster>
                    {
                        new StepMaster
                        {
                            StepId = 401,
                            StepName = "Reset sandbox data",
                            StepDescription = "Truncate sandbox tables and reseed fixtures",
                            ServerName = "sql-sandbox-01",
                            DatabaseName = "mktg_sandbox",
                            SpName = "sandbox.usp_reset_attribution",
                            ProcessId = 1003,
                            IsLastStep = 0,
                            DependentStepId = 0,
                            CreateDate = utcNow.AddDays(-5),
                            ExecutePath = "sql/reset-sandbox"
                        },
                        new StepMaster
                        {
                            StepId = 402,
                            StepName = "Apply draft rules",
                            StepDescription = "Run SSIS package that applies in-progress attribution logic",
                            ServerName = "ssis-farm-02",
                            DatabaseName = "mktg_sandbox",
                            SsisPackageName = "ApplyAttributionRules.dtsx",
                            SsisSolutionName = "AttributionSandbox.sln",
                            ProcessId = 1003,
                            IsLastStep = 0,
                            DependentStepId = 401,
                            CreateDate = utcNow.AddDays(-5),
                            ExecutePath = "ssis/attribution-sandbox"
                        },
                        new StepMaster
                        {
                            StepId = 403,
                            StepName = "Render QA dashboard",
                            StepDescription = "Build temp dashboards to review lift, fallout, and anomalies",
                            ServerName = "sql-sandbox-01",
                            DatabaseName = "mktg_sandbox",
                            SpName = "sandbox.usp_render_qa_views",
                            ProcessId = 1003,
                            IsLastStep = 1,
                            DependentStepId = 402,
                            CreateDate = utcNow.AddDays(-5),
                            ExecutePath = "sql/render-qa-dashboard"
                        }
                    }
                }
            };

            context.Agents.AddRange(agents);
            context.ProcessCatalog.AddRange(processes);
            context.SaveChanges();
        }
    }
}
