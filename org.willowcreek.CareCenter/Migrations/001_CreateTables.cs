using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 1, "1.5.0" )]
    class CreateTables : Migration
    {
        public override void Up()
        {
            AddAppointmentNotificationTable();

            AddServiceAreaTable();
            AddServiceAreaBanTable();
            AddServiceAreaAppointmentTimeslotTable();
            
            AddVisitTable();
            AddWorkflowAppointmentTable();

            AddAssessmentTable();
        }

        public void AddAppointmentNotificationTable()
        {
            Sql( @"
    CREATE TABLE [dbo].[_org_willowcreek_CareCenter_AppointmentNotification](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
	    [Name] [nvarchar](100) NOT NULL,
	    [FromName] [nvarchar](100) NULL,
	    [FromEmail] [nvarchar](100) NULL,
	    [SendReminders] [bit] NOT NULL,
	    [SendReminderDaysAhead] [nvarchar](100) NULL,
	    [ReminderSubject] [nvarchar](100) NULL,
	    [ReminderMessage] [nvarchar](max) NULL,
	    [SendAnnouncement] [bit] NOT NULL,
	    [AnnouncementSubject] [nvarchar](100) NULL,
	    [AnnouncementMessage] [nvarchar](max) NULL,
	    [CreatedDateTime] [datetime] NULL,
	    [ModifiedDateTime] [datetime] NULL,
	    [CreatedByPersonAliasId] [int] NULL,
	    [ModifiedByPersonAliasId] [int] NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [ForeignKey] [nvarchar](100) NULL,
	    [ForeignGuid] [uniqueidentifier] NULL,
	    [ForeignId] [int] NULL,
     CONSTRAINT [PK__org_willowcreek_CareCenter_AppointmentNotification] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_AppointmentNotification] ADD  CONSTRAINT [DF__org_willowcreek_CareCenter_AppointmentNotification_SendReminders]  DEFAULT ((0)) FOR [SendReminders]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_AppointmentNotification] ADD  CONSTRAINT [DF__org_willowcreek_CareCenter_AppointmentNotification_SendAppointmentAnnouncement]  DEFAULT ((0)) FOR [SendAnnouncement]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_AppointmentNotification] ADD  CONSTRAINT [DF___org_willo__Guid__2B4A5C8F]  DEFAULT (newid()) FOR [Guid]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_AppointmentNotification]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_AppointmentNotification_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_AppointmentNotification] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_AppointmentNotification_dbo.PersonAlias_CreatedByPersonAliasId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_AppointmentNotification]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_AppointmentNotification_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_AppointmentNotification] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_AppointmentNotification_dbo.PersonAlias_ModifiedByPersonAliasId]
" );
        }

        public void AddServiceAreaTable()
        {
            Sql( @"
    CREATE TABLE [dbo].[_org_willowcreek_CareCenter_ServiceArea](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
	    [Name] [nvarchar](100) NOT NULL,
	    [Order] [int] NOT NULL,
	    [CategoryId] [int] NULL,
	    [PassportLava] [nvarchar](max) NULL,
	    [IntakeLava] [nvarchar](max) NULL,
	    [WorkflowTypeId] [int] NULL,
	    [ScheduleId] [int] NULL,
	    [WorkflowAllowsScheduling] [bit] NOT NULL,
	    [IconCssClass] [nvarchar](100) NULL,
	    [UsesPassport] [bit] NOT NULL,
	    [CreatedDateTime] [datetime] NULL,
	    [ModifiedDateTime] [datetime] NULL,
	    [CreatedByPersonAliasId] [int] NULL,
	    [ModifiedByPersonAliasId] [int] NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [ForeignKey] [nvarchar](100) NULL,
	    [ForeignGuid] [uniqueidentifier] NULL,
	    [ForeignId] [int] NULL,
     CONSTRAINT [PK_org_willowcreek_CareCenter_ServiceArea] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceArea]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceArea_dbo.Category_CategoryId] FOREIGN KEY([CategoryId])
    REFERENCES [dbo].[Category] ([Id])

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceArea] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceArea_dbo.Category_CategoryId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceArea]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceArea_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceArea] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceArea_dbo.PersonAlias_CreatedByPersonAliasId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceArea]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceArea_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceArea] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceArea_dbo.PersonAlias_ModifiedByPersonAliasId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceArea]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceArea_dbo.Schedule_ScheduleId] FOREIGN KEY([ScheduleId])
    REFERENCES [dbo].[Schedule] ([Id])

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceArea] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceArea_dbo.Schedule_ScheduleId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceArea]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceArea_dbo.WorkflowType_WorkflowTypeId] FOREIGN KEY([WorkflowTypeId])
    REFERENCES [dbo].[WorkflowType] ([Id])

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceArea] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceArea_dbo.WorkflowType_WorkflowTypeId]
" );
        }

        public void AddServiceAreaBanTable()
        {
            Sql( @"
    CREATE TABLE [_org_willowcreek_CareCenter_ServiceAreaBan](
        [Id] [int] not null identity(1,1),
        [ServiceAreaId] [int] NOT NULL,
        [PersonAliasId] [int] NOT NULL,
	    [BanExpireDate] [datetime] NOT NULL,
        [CreatedDateTime] [datetime],
        [ModifiedDateTime] [datetime],
        [CreatedByPersonAliasId] [int],
        [ModifiedByPersonAliasId] [int],
        [Guid] uniqueidentifier not null DEFAULT NEWID(),
        [ForeignKey] nvarchar(100) null,
        [ForeignGuid] uniqueidentifier null,
        [ForeignId] [int] null
    CONSTRAINT [PK_org_willowcreek_CareCenter_ServiceAreaBan] PRIMARY KEY CLUSTERED ( [Id] ASC ) )

        ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaBan]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceAreaBan_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
            REFERENCES [dbo].[PersonAlias] ([Id])
        ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaBan] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceAreaBan_dbo.PersonAlias_PersonAliasId]

        ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaBan]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceAreaBan_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
            REFERENCES [dbo].[PersonAlias] ([Id])
        ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaBan] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceAreaBan_dbo.PersonAlias_CreatedByPersonAliasId]

        ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaBan]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceAreaBan_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
            REFERENCES [dbo].[PersonAlias] ([Id])
        ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaBan] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceAreaBan_dbo.PersonAlias_ModifiedByPersonAliasId]
        
        ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaBan]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceAreaBan_dbo._org_willowcreek_CareCenter_ServiceArea_ServiceAreaId] FOREIGN KEY([ServiceAreaId])
            REFERENCES [dbo].[_org_willowcreek_CareCenter_ServiceArea] ([Id])
            ON UPDATE CASCADE
            ON DELETE CASCADE
        ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaBan] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceAreaBan_dbo._org_willowcreek_CareCenter_ServiceArea_ServiceAreaId]

        CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[_org_willowcreek_CareCenter_ServiceAreaBan]([Guid])
        CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[_org_willowcreek_CareCenter_ServiceAreaBan]([CreatedByPersonAliasId])
        CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[_org_willowcreek_CareCenter_ServiceAreaBan]([ModifiedByPersonAliasId])
        " );
        }

        public void AddServiceAreaAppointmentTimeslotTable()
        {
            Sql( @"
    CREATE TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
	    [ServiceAreaId] [int] NOT NULL,
	    [NotificationId] [int] NULL,
	    [ScheduleId] [int] NULL,
	    [ScheduleTitle] [nvarchar](100) NOT NULL,
	    [DailyTitle] [nvarchar](100) NOT NULL,
	    [RegistrationLimit] [int] NULL,
	    [WalkupLimit] [int] NULL,
	    [AllowPublicRegistration] [bit] NOT NULL,
	    [IsActive] [bit] NOT NULL,
	    [CreatedDateTime] [datetime] NULL,
	    [ModifiedDateTime] [datetime] NULL,
	    [CreatedByPersonAliasId] [int] NULL,
	    [ModifiedByPersonAliasId] [int] NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [ForeignKey] [nvarchar](100) NULL,
	    [ForeignGuid] [uniqueidentifier] NULL,
	    [ForeignId] [int] NULL,
     CONSTRAINT [PK_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot]  WITH CHECK ADD  CONSTRAINT [FK__org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot__org_willowcreek_CareCenter_AppointmentNotification] FOREIGN KEY([NotificationId])
        REFERENCES [dbo].[_org_willowcreek_CareCenter_AppointmentNotification] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot] CHECK CONSTRAINT [FK__org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot__org_willowcreek_CareCenter_AppointmentNotification]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot]  WITH CHECK ADD  CONSTRAINT [FK__org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot__org_willowcreek_CareCenter_ServiceArea] FOREIGN KEY([ServiceAreaId])
        REFERENCES [dbo].[_org_willowcreek_CareCenter_ServiceArea] ([Id])
        ON UPDATE CASCADE
        ON DELETE CASCADE
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot] CHECK CONSTRAINT [FK__org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot__org_willowcreek_CareCenter_ServiceArea]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot]  WITH CHECK ADD  CONSTRAINT [FK__org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot_Schedule] FOREIGN KEY([ScheduleId])
        REFERENCES [dbo].[Schedule] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot] CHECK CONSTRAINT [FK__org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot_Schedule]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
        REFERENCES [dbo].[PersonAlias] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot_dbo.PersonAlias_CreatedByPersonAliasId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
        REFERENCES [dbo].[PersonAlias] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot_dbo.PersonAlias_ModifiedByPersonAliasId]
" );
        }

        public void AddVisitTable()
        {
            Sql( @"
    CREATE TABLE [_org_willowcreek_CareCenter_Visit](
        [Id] [int] not null identity(1,1),
        [PersonAliasId] [int] NOT NULL,
        [FamilyId] [int] NOT NULL,
	    [Status] [int] NOT NULL,
	    [CancelReasonValueId] [int] NULL,
	    [VisitDate] [datetime] NOT NULL,
        [PhotoIdValidated] [bit] NOT NULL DEFAULT 0,
        [ProofOfAddressValidated] [bit] NOT NULL DEFAULT 0,
	    [PagerId] [int] NULL,
	    [PassportStatus] [int] NOT NULL,
        [CreatedDateTime] [datetime],
        [ModifiedDateTime] [datetime],
        [CreatedByPersonAliasId] [int],
        [ModifiedByPersonAliasId] [int],
        [Guid] uniqueidentifier not null DEFAULT NEWID(),
        [ForeignKey] nvarchar(100) null,
        [ForeignGuid] uniqueidentifier null,
        [ForeignId] [int] null
    CONSTRAINT [PK_org_willowcreek_CareCenter_Visit] PRIMARY KEY CLUSTERED ( [Id] ASC ) )

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Visit]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Visit_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Visit] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Visit_dbo.PersonAlias_PersonAliasId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Visit]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Visit_dbo.Group_FamilyId] FOREIGN KEY([FamilyId])
    REFERENCES [dbo].[Group] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Visit] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Visit_dbo.Group_FamilyId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Visit]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Visit_dbo.DefinedValue_CancelReasonValueId] FOREIGN KEY([CancelReasonValueId])
    REFERENCES [dbo].[DefinedValue] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Visit] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Visit_dbo.DefinedValue_CancelReasonValueId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Visit]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Visit_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Visit] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Visit_dbo.PersonAlias_CreatedByPersonAliasId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Visit]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Visit_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Visit] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Visit_dbo.PersonAlias_ModifiedByPersonAliasId]

    CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[_org_willowcreek_CareCenter_Visit]([Guid])
    CREATE INDEX [IX_PersonAliasId] ON [dbo].[_org_willowcreek_CareCenter_Visit]([PersonAliasId])
    CREATE INDEX [IX_FamilyId] ON [dbo].[_org_willowcreek_CareCenter_Visit]([FamilyId])
    CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[_org_willowcreek_CareCenter_Visit]([CreatedByPersonAliasId])
    CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[_org_willowcreek_CareCenter_Visit]([ModifiedByPersonAliasId])

    CREATE TABLE [_org_willowcreek_CareCenter_VisitWorkflow](
        [VisitId] [int] NOT NULL,
        [WorkflowId] [int] NOT NULL
    CONSTRAINT [PK_org_willowcreek_CareCenter_VisitWorkflow] PRIMARY KEY CLUSTERED ( [VisitId] ASC, [WorkflowId] ASC ) )

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_VisitWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_VisitWorkflow_dbo.Visit_VisitId] FOREIGN KEY([VisitId])
        REFERENCES [dbo].[_org_willowcreek_CareCenter_Visit] ([Id])
        ON UPDATE CASCADE
        ON DELETE CASCADE
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_VisitWorkflow] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_VisitWorkflow_dbo.Visit_VisitId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_VisitWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_VisitWorkflow_dbo.Workflow_WorkflowId] FOREIGN KEY([WorkflowId])
        REFERENCES [dbo].[Workflow] ([Id])
        ON UPDATE CASCADE
        ON DELETE CASCADE
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_VisitWorkflow] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_VisitWorkflow_dbo.Workflow_WorkflowId]
" );
        }

        public void AddWorkflowAppointmentTable()
        {
            Sql( @"
    CREATE TABLE [dbo].[_org_willowcreek_CareCenter_WorkflowAppointment](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
	    [WorkflowId] [int] NOT NULL,
	    [TimeSlotId] [int] NOT NULL,
	    [PersonAliasId] [int] NOT NULL,
	    [AppointmentDate] [datetime] NOT NULL,
	    [AppointmentType] [int] NOT NULL,
	    [RemindersSent] [nvarchar](100) NULL,
	    [Status] [int] NOT NULL,
	    [CreatedDateTime] [datetime] NULL,
	    [ModifiedDateTime] [datetime] NULL,
	    [CreatedByPersonAliasId] [int] NULL,
	    [ModifiedByPersonAliasId] [int] NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [ForeignKey] [nvarchar](100) NULL,
	    [ForeignGuid] [uniqueidentifier] NULL,
	    [ForeignId] [int] NULL,
     CONSTRAINT [PK_org_willowcreek_CareCenter_WorkflowAppointment] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_WorkflowAppointment]  WITH CHECK ADD  CONSTRAINT [FK__org_willowcreek_CareCenter_WorkflowAppointment__org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot] FOREIGN KEY([TimeSlotId])
        REFERENCES [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_WorkflowAppointment] CHECK CONSTRAINT [FK__org_willowcreek_CareCenter_WorkflowAppointment__org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_WorkflowAppointment]  WITH CHECK ADD  CONSTRAINT [FK__org_willowcreek_CareCenter_WorkflowAppointment_Workflow] FOREIGN KEY([WorkflowId])
        REFERENCES [dbo].[Workflow] ([Id])
        ON UPDATE CASCADE
        ON DELETE CASCADE
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_WorkflowAppointment] CHECK CONSTRAINT [FK__org_willowcreek_CareCenter_WorkflowAppointment_Workflow]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_WorkflowAppointment]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_WorkflowAppointment_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
        REFERENCES [dbo].[PersonAlias] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_WorkflowAppointment] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_WorkflowAppointment_dbo.PersonAlias_CreatedByPersonAliasId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_WorkflowAppointment]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_WorkflowAppointment_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
        REFERENCES [dbo].[PersonAlias] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_WorkflowAppointment] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_WorkflowAppointment_dbo.PersonAlias_ModifiedByPersonAliasId]
" );
        }

        public void AddAssessmentTable()
        {
            Sql( @"
    CREATE TABLE [_org_willowcreek_CareCenter_Assessment](
        [Id] [int] not null identity(1,1),
        [VisitId] [int] NULL,
        [PersonAliasId] [int] NOT NULL,
        [FamilyId] [int] NOT NULL,
        [ApprovedByPersonAliasId] [int] NULL,
	    [AssessmentDate] [datetime] NOT NULL,
        [CreatedDateTime] [datetime],
        [ModifiedDateTime] [datetime],
        [CreatedByPersonAliasId] [int],
        [ModifiedByPersonAliasId] [int],
        [Guid] uniqueidentifier not null DEFAULT NEWID(),
        [ForeignKey] nvarchar(100) null,
        [ForeignGuid] uniqueidentifier null,
        [ForeignId] [int] null
    CONSTRAINT [PK_org_willowcreek_CareCenter_Assessment] PRIMARY KEY CLUSTERED ( [Id] ASC ) )

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Assessment]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Assessment_dbo.Visit_VisitId] FOREIGN KEY([VisitId])
    REFERENCES [dbo].[_org_willowcreek_CareCenter_Visit] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Assessment] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Assessment_dbo.Visit_VisitId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Assessment]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Assessment_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Assessment] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Assessment_dbo.PersonAlias_PersonAliasId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Assessment]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Assessment_dbo.Group_FamilyId] FOREIGN KEY([FamilyId])
    REFERENCES [dbo].[Group] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Assessment] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Assessment_dbo.Group_FamilyId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Assessment]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Assessment_dbo.PersonAlias_ApprovedByPersonAliasId] FOREIGN KEY([ApprovedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Assessment] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Assessment_dbo.PersonAlias_ApprovedByPersonAliasId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Assessment]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Assessment_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Assessment] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Assessment_dbo.PersonAlias_CreatedByPersonAliasId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Assessment]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Assessment_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Assessment] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Assessment_dbo.PersonAlias_ModifiedByPersonAliasId]

    CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[_org_willowcreek_CareCenter_Assessment]([Guid])
    CREATE INDEX [IX_PersonAliasId] ON [dbo].[_org_willowcreek_CareCenter_Assessment]([PersonAliasId])
    CREATE INDEX [IX_FamilyId] ON [dbo].[_org_willowcreek_CareCenter_Assessment]([FamilyId])
    CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[_org_willowcreek_CareCenter_Assessment]([CreatedByPersonAliasId])
    CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[_org_willowcreek_CareCenter_Assessment]([ModifiedByPersonAliasId])

    CREATE TABLE [_org_willowcreek_CareCenter_AssessmentWorkflow](
        [AssessmentId] [int] NOT NULL,
        [WorkflowId] [int] NOT NULL
    CONSTRAINT [PK_org_willowcreek_CareCenter_AssessmentWorkflow] PRIMARY KEY CLUSTERED ( [AssessmentId] ASC, [WorkflowId] ASC ) )

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_AssessmentWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_AssessmentWorkflow_dbo.Assessment_AssessmentId] FOREIGN KEY([AssessmentId])
        REFERENCES [dbo].[_org_willowcreek_CareCenter_Assessment] ([Id])
        ON UPDATE CASCADE
        ON DELETE CASCADE
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_AssessmentWorkflow] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_AssessmentWorkflow_dbo.Assessment_AssessmentId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_AssessmentWorkflow]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_AssessmentWorkflow_dbo.Workflow_WorkflowId] FOREIGN KEY([WorkflowId])
        REFERENCES [dbo].[Workflow] ([Id])
        ON UPDATE CASCADE
        ON DELETE CASCADE
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_AssessmentWorkflow] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_AssessmentWorkflow_dbo.Workflow_WorkflowId]
" );
        }


        public override void Down()
        {
        }

    }

}
