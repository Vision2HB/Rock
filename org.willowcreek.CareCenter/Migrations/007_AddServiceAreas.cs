using System;
using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 7, "1.5.0" )]
    class AddServiceAreas : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType( "org.willowcreek.CareCenter.Model.ServiceArea", "804EDF70-3A5F-4946-AD66-58DF87329ACA", true, true );

            RockMigrationHelper.UpdateCategory( "804EDF70-3A5F-4946-AD66-58DF87329ACA", "Food", "fa fa-cutlery", "", "7538CB73-5068-454A-880A-9ED5FA4263E7", 0 );
            RockMigrationHelper.UpdateCategory( "804EDF70-3A5F-4946-AD66-58DF87329ACA", "Clothing", "fa fa-tag", "", "3E20BFF0-DE53-41A5-BF62-010424E3C5C8", 1 );
            RockMigrationHelper.UpdateCategory( "804EDF70-3A5F-4946-AD66-58DF87329ACA", "Care Team", "fa fa-heartbeat", "", "51773E4E-14C9-4599-B1FF-7F57F9C45608", 2 );
            RockMigrationHelper.UpdateCategory( "804EDF70-3A5F-4946-AD66-58DF87329ACA", "Long-term Solutions", "fa fa-clipboard", "", "C9BAA441-7850-449C-889F-95F7A0F17416", 3 );
//            RockMigrationHelper.UpdateCategory( "804EDF70-3A5F-4946-AD66-58DF87329ACA", "Response Pastor", "fa fa-clipboard", "", "9E377ED9-46C0-438C-9782-FC95D821ADE2", 4 );
            RockMigrationHelper.UpdateCategory( "804EDF70-3A5F-4946-AD66-58DF87329ACA", "Clinic", "fa fa-ambulance", "", "25517EAB-A0FE-8CBE-4EEB-E163696C4E74", 5 );

            Sql( @"
    DECLARE @CategoryId INT
    DECLARE @WorkflowTypeId INT
    DECLARE @NotificationId INT
    DECLARE @ServiceAreaId INT
    DECLARE @ScheduleId INT

    DECLARE @StartDay DATETIME 
    DECLARE @Start DATETIME 
    DECLARE @End DATETIME 
    DECLARE @DowName VARCHAR(10)
	DECLARE @DailyTitle VARCHAR(100)
	DECLARE @Title VARCHAR(100)

    -- Add Notification
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_AppointmentNotification] 
        ( [Name], [FromName], [FromEmail], [SendReminders], [SendReminderDaysAhead], [ReminderSubject], 
            [ReminderMessage], [SendAnnouncement], [AnnouncementSubject], [AnnouncementMessage], [Guid] )
    VALUES 
        ( 'Default Notification', 'Willow Creek Care Center', 'info@willowcreekcarecenter.org', 1, 1, 'Care Center Reminder', 
'{{ Appointment.PersonAlias.Person.FullName }},<br/><br/>
This is a reminder of your <strong>{{ Appointment.TimeSlot.ServiceArea.Name }}</strong> appointment scheduled with
the Willow Creek Care Center on <strong>{{ Appointment.AppointmentDate| Date:''dddd, MMMM d, yyyy'' }}</strong>
at <strong>{{ Appointment.TimeSlot.ScheduleTitle | Date:''hh:mm tt'' }}</strong>.<br/>', 1, 'Apointment Scheduled', 
'{{ Appointment.PersonAlias.Person.FullName }},<br/><br/>
You have a <strong>{{ Appointment.TimeSlot.ServiceArea.Name }}</strong> appointment scheduled with
the Willow Creek Care Center on <strong>{{ Appointment.AppointmentDate| Date:''dddd, MMMM d, yyyy'' }}</strong>
at <strong>{{ Appointment.TimeSlot.ScheduleTitle | Date:''hh:mm tt'' }}</strong>.<br/>
',
'6211EA42-410D-4521-8CD4-9B4A5C2AD043' )
    SET @NotificationId = SCOPE_IDENTITY()

    -- Vision
    SET @CategoryId = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '25517EAB-A0FE-8CBE-4EEB-E163696C4E74' )
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
    VALUES ( 'Vision', 13, @CategoryId, '', null, 0, 0, '0D2D812F-196D-CA93-4218-61AF9CB937D9' )

    -- Dental
    SET @CategoryId = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '25517EAB-A0FE-8CBE-4EEB-E163696C4E74' )
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
    VALUES ( 'Dental', 12, @CategoryId, '', null, 0, 0, '54A0E3B9-5679-668C-412E-74343899B348' )

    -- Care Center Global
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
    VALUES ( 'Care Center Global', 11, null, '', null, 0, 0, '9687DD63-35B2-29B4-4EFB-EB3BAD4E218F' )

    -- Response Pastor
--    SET @CategoryId = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '9E377ED9-46C0-438C-9782-FC95D821ADE2' )
--    SET @WorkflowTypeId = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = 'A819E187-1980-4F90-81B5-415676643E06' )
--    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
--    VALUES ( 'Response Pastor', 10, @CategoryId, 
--        'Please let the guest know that a Response Pastor has been notified and will meet them in the Care Center as soon as they are available.', 
--        @WorkflowTypeId, 1, 0, 'C8EA27AD-2931-4119-ACE8-3C0F86AD7E9A' )

    -- Long Term Solutions
    SET @CategoryId = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'C9BAA441-7850-449C-889F-95F7A0F17416' )

    SET @WorkflowTypeId = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '6B37C278-5C83-49DF-86B7-3F4C51AB52CD' )
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
    VALUES ( 'Legal - Immigration', 9, @CategoryId, 
        'Please inform the guest that for privacy reasons, they will not receive any reminder of their appointment. If they need to cancel their appointment, they need to email/call. If they do not call/email to cancel, they''ll be ineligible to use our legal services for 6 months.',
        @WorkflowTypeId, 1, 0, '095E13E5-4F84-4D7E-B30D-5FE1AD69BD3E' )
    SET @ServiceAreaId = SCOPE_IDENTITY()

SET @StartDay = '2017-01-03 10:00:00'
WHILE @StartDay <= '2017-01-04'
BEGIN
	SET @Start = @StartDay
	WHILE @Start <= DATEADD( MINUTE, 100, @StartDay )
	BEGIN
		SET @End = DATEADD( MINUTE, 20, @Start )
		SET @DailyTitle = REPLACE(RIGHT(CONVERT( VARCHAR, @Start, 100 ),7),' ','')
		SET @Title = DATENAME( WEEKDAY, @Start) + ' ' + @DailyTitle
		INSERT INTO [dbo].[Schedule] ( [iCalendarContent], [Guid] )
		VALUES ( N'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:' + REPLACE(REPLACE( CONVERT( VARCHAR(19), @End, 126 ), ':', '' ),'-','') + '
DTSTART:' + REPLACE(REPLACE( CONVERT( VARCHAR(19), @Start, 126 ), ':', '' ),'-','') + '
RRULE:FREQ=MONTHLY;BYDAY=1TU
SEQUENCE:0
END:VEVENT
END:VCALENDAR', NEWID() )
		SET @ScheduleId = SCOPE_IDENTITY()
		INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot] 
			( [ServiceAreaId], [NotificationId], [ScheduleId], [ScheduleTitle], [DailyTitle], [RegistrationLimit], [WalkupLimit], [AllowPublicRegistration],[IsActive],[Guid] )
		VALUES ( @ServiceAreaId, NULL, @ScheduleId, @Title, @DailyTitle, 1, 0, 0, 1, NEWID() )
		SET @Start = @End
	END
	SET @StartDay = DATEADD( day, 1, @StartDay )
END

    SET @WorkflowTypeId = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '988FDF54-86D4-4CC3-BB4F-DE25A9D494B2' )
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
    VALUES ( 'Legal', 8, @CategoryId, 
        'Please inform the guest that for privacy reasons, they will not receive any reminder of their appointment. If they need to cancel their appointment, they need to email/call. If they do not call/email to cancel, they''ll be ineligible to use our legal services for 6 months.',
        @WorkflowTypeId, 1, 0, '2050BF96-9F8C-471B-B6E1-4151A5E428F9' )
    SET @ServiceAreaId = SCOPE_IDENTITY()

SET @StartDay = '2017-01-03 10:00:00'
WHILE @StartDay <= '2017-01-04'
BEGIN
	SET @Start = @StartDay
	WHILE @Start <= DATEADD( MINUTE, 90, @StartDay )
	BEGIN
		SET @End = DATEADD( MINUTE, 30, @Start )
		SET @DailyTitle = REPLACE(RIGHT(CONVERT( VARCHAR, @Start, 100 ),7),' ','')
		SET @Title = DATENAME( WEEKDAY, @Start) + ' ' + @DailyTitle
		INSERT INTO [dbo].[Schedule] ( [iCalendarContent], [Guid] )
		VALUES ( N'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:' + REPLACE(REPLACE( CONVERT( VARCHAR(19), @End, 126 ), ':', '' ),'-','') + '
DTSTART:' + REPLACE(REPLACE( CONVERT( VARCHAR(19), @Start, 126 ), ':', '' ),'-','') + '
RRULE:FREQ=WEEKLY;BYDAY=' + UPPER(LEFT(@Title,2)) + '
SEQUENCE:0
END:VEVENT
END:VCALENDAR', NEWID() )
		SET @ScheduleId = SCOPE_IDENTITY()
		INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot] 
			( [ServiceAreaId], [NotificationId], [ScheduleId], [ScheduleTitle], [DailyTitle], [RegistrationLimit], [WalkupLimit], [AllowPublicRegistration],[IsActive],[Guid] )
		VALUES ( @ServiceAreaId, NULL, @ScheduleId, @Title, @DailyTitle, 2, 0, 0, 1, NEWID() )
		SET @Start = @End
	END
	SET @StartDay = DATEADD( day, 1, @StartDay )
END

SET @StartDay = '2017-01-03 18:30:00'
WHILE @StartDay <= '2017-01-04'
BEGIN
	SET @Start = @StartDay
	WHILE @Start <= DATEADD( MINUTE, 30, @StartDay )
	BEGIN
		SET @End = DATEADD( MINUTE, 30, @Start )
		SET @DailyTitle = REPLACE(RIGHT(CONVERT( VARCHAR, @Start, 100 ),7),' ','')
		SET @Title = DATENAME( WEEKDAY, @Start) + ' ' + @DailyTitle
		INSERT INTO [dbo].[Schedule] ( [iCalendarContent], [Guid] )
		VALUES ( N'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:' + REPLACE(REPLACE( CONVERT( VARCHAR(19), @End, 126 ), ':', '' ),'-','') + '
DTSTART:' + REPLACE(REPLACE( CONVERT( VARCHAR(19), @Start, 126 ), ':', '' ),'-','') + '
RRULE:FREQ=WEEKLY;BYDAY=' + UPPER(LEFT(@Title,2)) + '
SEQUENCE:0
END:VEVENT
END:VCALENDAR', NEWID() )
		SET @ScheduleId = SCOPE_IDENTITY()
		INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot] 
			( [ServiceAreaId], [NotificationId], [ScheduleId], [ScheduleTitle], [DailyTitle], [RegistrationLimit], [WalkupLimit], [AllowPublicRegistration],[IsActive],[Guid] )
		VALUES ( @ServiceAreaId, NULL, @ScheduleId, @Title, @DailyTitle, 2, 0, 0, 1, NEWID() )
		SET @Start = @End
	END
	SET @StartDay = DATEADD( day, 1, @StartDay )
END

    SET @WorkflowTypeId = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '2A76CDB2-F330-4C9B-A1E8-24308041CB03' )
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
    VALUES ( 'Financial Coaching', 7, @CategoryId, '', @WorkflowTypeId, 1, 0, '64168E99-9C1D-48BC-AF7C-BF72E4A44928' )
    SET @ServiceAreaId = SCOPE_IDENTITY()

/*
SET @StartDay = '2016-10-03 08:00:00'
WHILE @StartDay <= '2016-10-07 08:00:00'
BEGIN
	SET @Start = @StartDay
	WHILE @Start <= DATEADD( HOUR, 8, @StartDay )
	BEGIN
		SET @End = DATEADD( HOUR, 1, @Start )
		SET @DailyTitle = REPLACE(RIGHT(CONVERT( VARCHAR, @Start, 100 ),7),' ','')
		SET @Title = DATENAME( WEEKDAY, @Start) + ' ' + @DailyTitle
		INSERT INTO [dbo].[Schedule] ( [iCalendarContent], [Guid] )
		VALUES ( N'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:' + REPLACE(REPLACE( CONVERT( VARCHAR(19), @End, 126 ), ':', '' ),'-','') + '
DTSTART:' + REPLACE(REPLACE( CONVERT( VARCHAR(19), @Start, 126 ), ':', '' ),'-','') + '
RRULE:FREQ=WEEKLY;BYDAY=' + UPPER(LEFT(@Title,2)) + '
SEQUENCE:0
END:VEVENT
END:VCALENDAR', NEWID() )
		SET @ScheduleId = SCOPE_IDENTITY()
		INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot] 
			( [ServiceAreaId], [NotificationId], [ScheduleId], [ScheduleTitle], [DailyTitle], [RegistrationLimit], [WalkupLimit], [AllowPublicRegistration],[IsActive],[Guid] )
		VALUES ( @ServiceAreaId, @NotificationId, @ScheduleId, @Title, @DailyTitle, 3, 0, 0, 1, NEWID() )
		SET @Start = @End
	END
	SET @StartDay = DATEADD( day, 1, @StartDay )
END
*/

    SET @WorkflowTypeId = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '9196C4D9-A820-4217-9341-44B2C0084F74' )
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
    VALUES ( 'Employment', 6, @CategoryId, '', @WorkflowTypeId, 1, 0, 'C20E771F-AE2E-49C7-B31D-C0D63214C74E' )
    SET @ServiceAreaId = SCOPE_IDENTITY()

/*
SET @StartDay = '2016-10-03 08:00:00'
WHILE @StartDay <= '2016-10-07 08:00:00'
BEGIN
	SET @Start = @StartDay
	WHILE @Start <= DATEADD( HOUR, 8, @StartDay )
	BEGIN
		SET @End = DATEADD( HOUR, 1, @Start )
		SET @DailyTitle = REPLACE(RIGHT(CONVERT( VARCHAR, @Start, 100 ),7),' ','')
		SET @Title = DATENAME( WEEKDAY, @Start) + ' ' + @DailyTitle
		INSERT INTO [dbo].[Schedule] ( [iCalendarContent], [Guid] )
		VALUES ( N'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:' + REPLACE(REPLACE( CONVERT( VARCHAR(19), @End, 126 ), ':', '' ),'-','') + '
DTSTART:' + REPLACE(REPLACE( CONVERT( VARCHAR(19), @Start, 126 ), ':', '' ),'-','') + '
RRULE:FREQ=WEEKLY;BYDAY=' + UPPER(LEFT(@Title,2)) + '
SEQUENCE:0
END:VEVENT
END:VCALENDAR', NEWID() )
		SET @ScheduleId = SCOPE_IDENTITY()
		INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot] 
			( [ServiceAreaId], [NotificationId], [ScheduleId], [ScheduleTitle], [DailyTitle], [RegistrationLimit], [WalkupLimit], [AllowPublicRegistration],[IsActive],[Guid] )
		VALUES ( @ServiceAreaId, @NotificationId, @ScheduleId, @Title, @DailyTitle, 3, 0, 0, 1, NEWID() )
		SET @Start = @End
	END
	SET @StartDay = DATEADD( day, 1, @StartDay )
END
*/

/*
    SET @WorkflowTypeId = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = 'AC5D1D53-2E45-44E2-8069-C770C2530538' )
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
    VALUES ( 'Tax Prep (VITA)', 4, @CategoryId, '', @WorkflowTypeId, 1, 0, '52E00962-B455-485B-ADD1-9BE5D67574D4' )
    SET @ServiceAreaId = SCOPE_IDENTITY()

SET @StartDay = '2016-10-03 08:00:00'
WHILE @StartDay <= '2016-10-07 08:00:00'
BEGIN
	SET @Start = @StartDay
	WHILE @Start <= DATEADD( HOUR, 8, @StartDay )
	BEGIN
		SET @End = DATEADD( HOUR, 1, @Start )
		SET @DailyTitle = REPLACE(RIGHT(CONVERT( VARCHAR, @Start, 100 ),7),' ','')
		SET @Title = DATENAME( WEEKDAY, @Start) + ' ' + @DailyTitle
		INSERT INTO [dbo].[Schedule] ( [iCalendarContent], [Guid] )
		VALUES ( N'BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:' + REPLACE(REPLACE( CONVERT( VARCHAR(19), @End, 126 ), ':', '' ),'-','') + '
DTSTART:' + REPLACE(REPLACE( CONVERT( VARCHAR(19), @Start, 126 ), ':', '' ),'-','') + '
RRULE:FREQ=WEEKLY;BYDAY=' + UPPER(LEFT(@Title,2)) + '
SEQUENCE:0
END:VEVENT
END:VCALENDAR', NEWID() )
		SET @ScheduleId = SCOPE_IDENTITY()
		INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot] 
			( [ServiceAreaId], [NotificationId], [ScheduleId], [ScheduleTitle], [DailyTitle], [RegistrationLimit], [WalkupLimit], [AllowPublicRegistration],[IsActive],[Guid] )
		VALUES ( @ServiceAreaId, @NotificationId, @ScheduleId, @Title, @DailyTitle, 3, 0, 0, 1, NEWID() )
		SET @Start = @End
	END
	SET @StartDay = DATEADD( day, 1, @StartDay )
END
*/

    -- Care Team
    SET @CategoryId = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '51773E4E-14C9-4599-B1FF-7F57F9C45608' )
    SET @WorkflowTypeId = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = 'CFA47B9F-1380-4705-B50B-F0B122B386A3' )
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
    VALUES ( 'Care Team', 5, @CategoryId, 
        'For an assessment, please remind the guest that it''s helpful if they have proof of income (Social Security payment, pay stub, unemployment benefits statement, bank statement, tax statement) with them when meeting with the Care Team.',
        @WorkflowTypeId, 1, 0, '203CD767-CD7C-4CDE-A57D-162EA4FD0008' )

    SET @WorkflowTypeId = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = 'EA47030D-D539-4509-BA46-A57ED61D61E4' )
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
    VALUES ( 'Resource', 4, @CategoryId, '', @WorkflowTypeId, 1, 0, '60157D50-94C8-4747-96B5-F03307FE6F10' )

    -- Clothing
    SET @CategoryId = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '3E20BFF0-DE53-41A5-BF62-010424E3C5C8' )
    SET @WorkflowTypeId = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '99830D9E-E8AF-4757-A84E-2D4C4EC5BE09' )
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
    VALUES ( 'Clothing Visit', 3, @CategoryId, 
        'Please remind the guest that they can pay with CASH or DEBIT only.', 
        @WorkflowTypeId, 0, 1, '32B9C517-AC4A-4FFA-AD17-61967C1CA1F6' )

    SET @WorkflowTypeId = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '46B04B7C-699C-4513-804E-BA54A11B2AF9' )
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
    VALUES ( 'Limited Clothing Visit', 2, @CategoryId, 
        'Payable with CASH or DEBIT only, and is limited to 10 items total.', 
        @WorkflowTypeId, 0, 1, 'DC2F126F-EB6A-47D3-8F44-C473035B8C53' )

    -- Food
    SET @CategoryId = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '7538CB73-5068-454A-880A-9ED5FA4263E7' )
    SET @WorkflowTypeId = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = 'BCE8C80C-81FF-4580-9000-A09A4B8920E3' )
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
    VALUES ( 'Food Visit', 1, @CategoryId, '', @WorkflowTypeId, 0, 1, 'B8154500-F8B2-4D7E-8926-196BB6DB9A82' )

    SET @WorkflowTypeId = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '2A5ACB4A-69DE-4D16-BDA3-C7187297D3B9' )
    INSERT INTO [dbo].[_org_willowcreek_CareCenter_ServiceArea] ( [Name], [Order], [CategoryId], [IntakeLava], [WorkflowTypeId], [WorkflowAllowsScheduling], [UsesPassport], [Guid] )
    VALUES ( 'Bread Visit', 0, @CategoryId, '', @WorkflowTypeId, 0, 1, '3DA2915D-6D74-47B3-8B8A-F1B6041F9546' )

" );
        }

        public override void Down()
        {
        }
    }
}
