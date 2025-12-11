-- DataPulse database schema
CREATE TABLE [dbo].[Task] (
    [TaskId] INT IDENTITY(1,1) PRIMARY KEY,
    [TaskName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(1000) NULL,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'NotStarted',
    [LastRunStartTime] DATETIME2 NULL,
    [LastRunEndTime] DATETIME2 NULL,
    [LastRunBy] NVARCHAR(100) NULL
);

CREATE TABLE [dbo].[Process] (
    [ProcessId] INT IDENTITY(1,1) PRIMARY KEY,
    [TaskId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Task]([TaskId]),
    [ProcessName] NVARCHAR(200) NOT NULL,
    [ProcessType] NVARCHAR(50) NOT NULL,
    [ExecutionOrder] INT NOT NULL,
    [TargetIdentifier] NVARCHAR(500) NOT NULL,
    [ParametersJson] NVARCHAR(MAX) NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'NotStarted',
    [StartTime] DATETIME2 NULL,
    [EndTime] DATETIME2 NULL,
    [LastErrorMessage] NVARCHAR(MAX) NULL,
    [LastRunBy] NVARCHAR(100) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1
);

CREATE UNIQUE INDEX IX_Process_Task_Order ON [dbo].[Process]([TaskId], [ExecutionOrder]);

-- Existing process catalog tables
CREATE TABLE [process].[process_master]
(
    [process_id] INT NOT NULL IDENTITY(10001, 113),
    [process_name] VARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [process_description] VARCHAR(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [dependent_process_Id] INT NOT NULL CONSTRAINT [DF__process_m__depen__787F9781] DEFAULT ((0)),
    [process_type_id] INT NOT NULL CONSTRAINT [DF__process_m__proce__547407D2] DEFAULT ((0)),
    [agent_master_id] BIGINT NULL,
    [from_email_address] VARCHAR(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [success_to_email_address] VARCHAR(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [success_cc_email_address] VARCHAR(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [failure_to_email_address] VARCHAR(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [failure_cc_email_address] VARCHAR(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [send_email_on_success] BIT NULL,
    [send_email_on_failure] BIT NULL,
    [can_be_rerun] BIT NULL,
    [has_event_handler] BIT NULL,
    [has_exceptions] BIT NULL,
    [create_date] SMALLDATETIME NULL CONSTRAINT [DF__process_m__creat__3B7151A8] DEFAULT (getdate()),
    [delete_date] SMALLDATETIME NULL,
    [is_critical] BIT NOT NULL CONSTRAINT [DF__process_m__is_cr__2541063B] DEFAULT ((0))
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
ALTER TABLE [process].[process_master] ADD CONSTRAINT [PK__test__process_master] PRIMARY KEY CLUSTERED ([process_id]) WITH (FILLFACTOR=100) ON [PRIMARY];
ALTER TABLE [process].[process_master] ADD CONSTRAINT [fk_process__process_master__agent_master_id] FOREIGN KEY ([agent_master_id]) REFERENCES [sql_job].[agent_master] ([agent_master_id]);
ALTER TABLE [process].[process_master] ADD CONSTRAINT [FK_process__process_master__process_types] FOREIGN KEY ([process_type_id]) REFERENCES [process].[process_types] ([process_type_id]);

CREATE TABLE [process].[step_master]
(
    [step_id] INT NOT NULL IDENTITY(100, 1),
    [step_name] VARCHAR(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [step_description] VARCHAR(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [server_name] VARCHAR(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [database_name] VARCHAR(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [sp_name] VARCHAR(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ssis_package_name] NVARCHAR(250) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ssis_solution_name] NVARCHAR(250) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [process_id] INT NOT NULL,
    [is_last_step] TINYINT NOT NULL CONSTRAINT [DF__step_mast__is_la__381975C0] DEFAULT ((0)),
    [dependent_step_id] INT NOT NULL CONSTRAINT [DF__step_mast__depen__5D264C0] DEFAULT ((0)),
    [create_date] SMALLDATETIME NOT NULL CONSTRAINT [DF__step_mast__creat__2C7E836D] DEFAULT (getdate()),
    [delete_date] SMALLDATETIME NULL,
    [execute_path] VARCHAR(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
ALTER TABLE [process].[step_master] ADD CONSTRAINT [PK_process__step_master] PRIMARY KEY CLUSTERED ([step_id]) WITH (FILLFACTOR=100) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [nci__step_master__process_id] ON [process].[step_master] ([process_id]) WITH (FILLFACTOR=80) ON [PRIMARY];
ALTER TABLE [process].[step_master] ADD CONSTRAINT [FK_process__step_master__process_id] FOREIGN KEY ([process_id]) REFERENCES [process].[process_master] ([process_id]);

CREATE TABLE [sql_job].[agent_master]
(
    [agent_master_id] BIGINT NOT NULL,
    [agent_master_name] VARCHAR(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [create_date] DATETIME2(3) NULL CONSTRAINT [DF__agent_mas__creat__75586032] DEFAULT (getdate()),
    [delete_date] DATETIME2(3) NULL
) ON [PRIMARY];
ALTER TABLE [sql_job].[agent_master] ADD CONSTRAINT [ck__sql_job__agent_master_id] CHECK (([agent_master_id] like '99%' OR [agent_master_id] like '100099%' OR [agent_master_id] like '300099%'));
ALTER TABLE [sql_job].[agent_master] ADD CONSTRAINT [pk__sql_job__agent_master_id] PRIMARY KEY CLUSTERED ([agent_master_id]) WITH (FILLFACTOR=100) ON [PRIMARY];
