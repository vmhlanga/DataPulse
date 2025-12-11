using DataPulse.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataPulse.Infrastructure.Data
{
    public class DataPulseDbContext : DbContext
    {
        public DataPulseDbContext(DbContextOptions<DataPulseDbContext> options) : base(options)
        {
        }

        public DbSet<DataTask> Tasks => Set<DataTask>();
        public DbSet<Process> Processes => Set<Process>();
        public DbSet<ProcessMaster> ProcessCatalog => Set<ProcessMaster>();
        public DbSet<StepMaster> StepCatalog => Set<StepMaster>();
        public DbSet<AgentMaster> Agents => Set<AgentMaster>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataTask>(entity =>
            {
                entity.HasKey(t => t.TaskId);
                entity.Property(t => t.TaskName).HasMaxLength(200).IsRequired();
                entity.Property(t => t.Status).HasMaxLength(50).IsRequired();
                entity.Property(t => t.CreatedBy).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<Process>(entity =>
            {
                entity.HasKey(p => p.ProcessId);
                entity.Property(p => p.ProcessName).HasMaxLength(200).IsRequired();
                entity.Property(p => p.ProcessType).HasConversion<string>().HasMaxLength(50);
                entity.Property(p => p.Status).HasMaxLength(50).HasDefaultValue("NotStarted");
                entity.Property(p => p.TargetIdentifier).HasMaxLength(500);
                entity.HasOne(p => p.Task)
                    .WithMany(t => t.Processes)
                    .HasForeignKey(p => p.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(p => new { p.TaskId, p.ExecutionOrder }).IsUnique();
            });

            modelBuilder.Entity<ProcessMaster>(entity =>
            {
                entity.ToTable("process_master", "process");
                entity.HasKey(p => p.ProcessId);
                entity.Property(p => p.ProcessId).HasColumnName("process_id");
                entity.Property(p => p.ProcessName).HasColumnName("process_name").HasMaxLength(100).IsRequired();
                entity.Property(p => p.ProcessDescription).HasColumnName("process_description");
                entity.Property(p => p.DependentProcessId).HasColumnName("dependent_process_Id");
                entity.Property(p => p.ProcessTypeId).HasColumnName("process_type_id");
                entity.Property(p => p.AgentMasterId).HasColumnName("agent_master_id");
                entity.Property(p => p.FromEmailAddress).HasColumnName("from_email_address").HasMaxLength(500);
                entity.Property(p => p.SuccessToEmailAddress).HasColumnName("success_to_email_address").HasMaxLength(500);
                entity.Property(p => p.SuccessCcEmailAddress).HasColumnName("success_cc_email_address").HasMaxLength(500);
                entity.Property(p => p.FailureToEmailAddress).HasColumnName("failure_to_email_address").HasMaxLength(500);
                entity.Property(p => p.FailureCcEmailAddress).HasColumnName("failure_cc_email_address").HasMaxLength(500);
                entity.Property(p => p.SendEmailOnSuccess).HasColumnName("send_email_on_success");
                entity.Property(p => p.SendEmailOnFailure).HasColumnName("send_email_on_failure");
                entity.Property(p => p.CanBeRerun).HasColumnName("can_be_rerun");
                entity.Property(p => p.HasEventHandler).HasColumnName("has_event_handler");
                entity.Property(p => p.HasExceptions).HasColumnName("has_exceptions");
                entity.Property(p => p.CreateDate).HasColumnName("create_date");
                entity.Property(p => p.DeleteDate).HasColumnName("delete_date");
                entity.Property(p => p.IsCritical).HasColumnName("is_critical");

                entity.HasOne(p => p.Agent)
                    .WithMany()
                    .HasForeignKey(p => p.AgentMasterId);
            });

            modelBuilder.Entity<StepMaster>(entity =>
            {
                entity.ToTable("step_master", "process");
                entity.HasKey(s => s.StepId);
                entity.Property(s => s.StepId).HasColumnName("step_id");
                entity.Property(s => s.StepName).HasColumnName("step_name").HasMaxLength(1000).IsRequired();
                entity.Property(s => s.StepDescription).HasColumnName("step_description");
                entity.Property(s => s.ServerName).HasColumnName("server_name").HasMaxLength(1000);
                entity.Property(s => s.DatabaseName).HasColumnName("database_name").HasMaxLength(1000);
                entity.Property(s => s.SpName).HasColumnName("sp_name").HasMaxLength(1000);
                entity.Property(s => s.SsisPackageName).HasColumnName("ssis_package_name").HasMaxLength(250);
                entity.Property(s => s.SsisSolutionName).HasColumnName("ssis_solution_name").HasMaxLength(250);
                entity.Property(s => s.ProcessId).HasColumnName("process_id");
                entity.Property(s => s.IsLastStep).HasColumnName("is_last_step");
                entity.Property(s => s.DependentStepId).HasColumnName("dependent_step_id");
                entity.Property(s => s.CreateDate).HasColumnName("create_date");
                entity.Property(s => s.DeleteDate).HasColumnName("delete_date");
                entity.Property(s => s.ExecutePath).HasColumnName("execute_path");

                entity.HasOne(s => s.Process)
                    .WithMany(p => p.Steps)
                    .HasForeignKey(s => s.ProcessId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AgentMaster>(entity =>
            {
                entity.ToTable("agent_master", "sql_job");
                entity.HasKey(a => a.AgentMasterId);
                entity.Property(a => a.AgentMasterId).HasColumnName("agent_master_id");
                entity.Property(a => a.AgentMasterName).HasColumnName("agent_master_name").HasMaxLength(500);
                entity.Property(a => a.CreateDate).HasColumnName("create_date");
                entity.Property(a => a.DeleteDate).HasColumnName("delete_date");
            });
        }
    }
}
