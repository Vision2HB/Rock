/****** Object:  StoredProcedure [dbo].[_com_bemaservices_spCloneGroups]    Script Date: 6/19/2020 2:48:21 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/*
<doc>
	<summary>
		This stored procedure creates a duplicate of all Groups specified inside of a Group Tree Structure in a new group type.  
		It will create new Group, Group Location, Group Location Schedule, Location, Schedule, Group Sync, Group Attribute Values, Group Member Attributes and Group Member Records.
	</summary>

	<param name = 'RootGroupId' datatype='Int'>The Group Tree you'd like to Duplicate</param>
	<param name = 'OldGroupTypeId' datatype='Int'>This is the group type of the old group tree.  Note that this procedure requires a homogenous tree of all one group type</param>
	<param name = 'NewGroupTypeId' datatype='Int'>The new group type</param>
	<param name = 'NewParentGroupId' datatype='Int'>The parent group id to place new groups under</param>
	<param name = 'CopyAttendanceYN' datatype='bit'>Should attendance from the old groups be copied to the new groups</param>
	<param name = 'DeleteExistingYN' datatype='bit'>Should existing groups of the new group type by the same name be deleted</param>
	<param name = 'IncludeRootGroup' datatype='bit'>Should root group be included and placed under the new parent group</param>
	<param name = 'RoleMap' datatype='nvarchar(250)'>A comma and | separated list of old role ids and new role ids like '25,100|26,102'</param>
	<param name = 'AttributeMap' dataype='nvarchar(250)'>A comma and | separated list of old attribute ids and new attribute ids like '25,100|26,102'</param>
	<param name = 'MemberAttributeMap' dataype='nvarchar(250)'>A comma and | separated list of old attribute ids and new attribute ids like '25,100|26,102'</param>
    <param name = 'CopyChildrenYN' datatype='bit'>Should descendant groups be copied along with selected root group</param>
 </doc>
*/
CREATE OR ALTER PROCEDURE [dbo].[_com_bemaservices_spMigrateGroups]
(
    @rootGroupId int
	, @oldGroupTypeId int
	, @newGroupTypeId int
	, @newParentGroupId int
	, @copyAttendanceYN bit = 0
	, @deleteExistingYN bit = 0
    , @copyChildrenYN bit = 1
	, @roleMap varchar(250) = ''
	, @attributeMap varchar(250) = ''
	, @memberAttributeMap varchar(250) = ''
	, @includeRootGroup bit = 1
	, @personId int = 1
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

    -- Insert statements for procedure here
    DECLARE @ForeignKey nvarchar(max) = 'MigratedGroup';
	DECLARE @parentGroupName nvarchar(250);
	DECLARE @parentParentGroupId int;

    DECLARE @GroupIds TABLE ( Id int )

    -- If copy children is Yes
    IF @copyChildrenYN = 1
    Begin
	;With CTE as (
		Select g.Id, g.parentGroupId
		From [Group] g
		Where g.Id = @rootGroupId

		UNION ALL

		Select g.Id, g.parentGroupId
		From [Group] g
		Join CTE on g.ParentGroupId = cte.Id And g.IsActive = 1 and g.IsArchived = 0
	)

    INSERT INTO @GroupIds
	Select
		Id
	From CTE
    END

    -- If copy children is No
    IF @copyChildrenYN = 0
    BEGIN

    INSERT INTO @GroupIds
    VALUES ( @rootGroupId )

    END

	/* handle root group differently 
	   If root group goes under parent, set the parent group id to the @newParentGroupId
	   If merging the root and parent group, link the root group to the right parent and keep the name of the group the same (no disappearing groups) and ALSO
	      put the ForeignKey and Id to point the the root group so they will properly merge.
		STORE INFO FOR LATER USE.
	*/
	IF @includeRootGroup = 1
	Begin
	    SELECT @parentGroupName = Name From [Group] Where Id =  @newParentGroupId
		SELECT @parentParentGroupId = ParentGroupId From [Group] Where Id =  @newParentGroupId
	End

	/* Delete Groups */
	IF @deleteExistingYN = 1
	Begin

		Delete From Attendance 
		    Where OccurrenceId in (Select Id from AttendanceOccurrence 
				Where GroupId in (Select Id From [Group] Where ForeignKey = @ForeignKey
				And ForeignId in ( Select Id from @GroupIds ) ) )

		Delete from AttendanceOccurrence 
			Where GroupId in (Select Id From [Group] Where ForeignKey = @ForeignKey
			And ForeignId in ( Select Id from @GroupIds ) )

		Delete From GroupMember Where ForeignKey = @ForeignKey
			And ForeignId in ( Select Id From GroupMember Where GroupId in ( Select Id From [Group] Where ForeignKey = @ForeignKey
			And ForeignId in ( Select Id from @GroupIds ) ) )

		Delete From [Group] Where ForeignKey = @ForeignKey
			And ForeignId in ( Select Id from @GroupIds )

	End


	/*
		INSERT GROUPS
	*/

DECLARE @TargetGroup TABLE(Id int NOT NULL,
 	[IsSystem] [bit] NOT NULL,
	[ParentGroupId] [int] NULL,
	[GroupTypeId] [int] NOT NULL,
	[CampusId] [int] NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[IsSecurityRole] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Order] [int] NOT NULL,
	[Guid] [uniqueidentifier] NOT NULL,
	[CreatedDateTime] [datetime] NULL,
	[ModifiedDateTime] [datetime] NULL,
	[CreatedByPersonAliasId] [int] NULL,
	[ModifiedByPersonAliasId] [int] NULL,
	[ForeignKey] [nvarchar](100) NULL,
	[AllowGuests] [bit] NULL,
	[ScheduleId] [int] NULL,
	[IsPublic] [bit] NOT NULL,
	[ForeignGuid] [uniqueidentifier] NULL,
	[ForeignId] [int] NULL,
	[GroupCapacity] [int] NULL,
	[RequiredSignatureDocumentTemplateId] [int] NULL,
	[InactiveDateTime] [datetime] NULL,
	[IsArchived] [bit] NOT NULL,
	[ArchivedDateTime] [datetime] NULL,
	[ArchivedByPersonAliasId] [int] NULL,
	[StatusValueId] [int] NULL,
	[GroupAdministratorPersonAliasId] [int] NULL,
	[SchedulingMustMeetRequirements] [bit] NOT NULL,
	[AttendanceRecordRequiredForCheckIn] [int] NOT NULL,
	[ScheduleCancellationPersonAliasId] [int] NULL,
	[InactiveReasonValueId] [int] NULL,
	[InactiveReasonNote] [nvarchar](max) NULL,
	[RSVPReminderSystemCommunicationId] [int] NULL,
	[RSVPReminderOffsetDays] [int] NULL )

	Insert Into @TargetGroup (
		Id
		,IsSystem
		, GroupTypeId
		, ParentGroupId
		, CampusId
		, [Name]
		, [Description]
		, ScheduleId
		, IsSecurityRole
		, IsActive
		, [Order]
		, [Guid]
		, CreatedDateTime
		, CreatedByPersonAliasId
		, ModifiedDateTime
		, ModifiedByPersonAliasId
		, ForeignKey
		, ForeignId
		, AllowGuests
		, IsPublic
		, GroupCapacity
		, IsArchived
		, [SchedulingMustMeetRequirements]
		, [AttendanceRecordRequiredForCheckIn] )
	Select
		g.Id
		, IsSystem = 0
		, GroupTypeId = @newGroupTypeId
		, ParentGroupId = g.ParentGroupId
		, CampusId = g.CampusId
		, [Name] = g.[Name]
		, [Description] = g.[Description]
		, ScheduleId = g.ScheduleId
		, IsSecurityRole = g.IsSecurityRole
		, IsActive = g.IsActive
		, [Order] = g.[Order]
		, [Guid] = NEWID()
		, CreatedDateTime = GetDate()
		, CreatedByPersonAliasId = dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
		, ModifiedDateTime = GetDate()
		, ModifiedByPersonAliasId = dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
		, ForeignKey = @ForeignKey
		, ForeignId = g.Id
		, AllowGuests = g.AllowGuests
		, IsPublic = g.IsPublic
		, GroupCapacity = g.GroupCapacity
		, g.IsArchived
		, g.[SchedulingMustMeetRequirements]
		, g.[AttendanceRecordRequiredForCheckIn]
	From [Group] g
	LEFT Join [group] g2 on g2.ForeignId = g.Id 
				and g2.ForeignKey = @foreignKey
				and g2.GroupTypeId = @NewGroupTypeId
	Where 
		g.Id in ( Select Id From @GroupIds )
		AND g.IsArchived = 0
		and g2.Id is Null

	/* handle root group differently 
	   If root group goes under parent, set the parent group id to the @newParentGroupId
	   If merging the root and parent group, link the root group to the right parent and keep the name of the group the same (no disappearing groups) and ALSO
	      put the ForeignKey and Id to point the the root group so they will properly merge.
	*/
	IF @includeRootGroup != 1
		Update @TargetGroup Set ParentGroupId = @newParentGroupId WHERE Id = @rootGroupId
	Else
	Begin
	    Update @TargetGroup Set Name = @parentGroupName
		, ParentGroupId = @parentParentGroupId WHERE Id = @rootGroupId
		IF @deleteExistingYN = 0
			Update [Group] Set ForeignKey = @foreignKey, ForeignId = @rootGroupId Where Id = @newParentGroupId
	End


	/* Insert or Update Group Table */
	MERGE [Group] g
	USING @TargetGroup tg ON g.GroupTypeId = tg.GroupTypeId and g.ForeignKey = @foreignKey and g.ForeignId = tg.Id
	WHEN MATCHED THEN
		UPDATE SET g.IsSystem = tg.IsSystem
		, g.GroupTypeId = tg.GroupTypeId
		, g.ParentGroupId = tg.ParentGroupId
		, g.CampusId = tg.CampusId
		, g.[Name] = tg.[Name]
		, g.[Description] = tg.[Description] 
		, g.ScheduleId = tg.ScheduleId
		, g.IsSecurityRole = tg.IsSecurityRole
		, g.IsActive = tg.IsActive
		, g.[Order] = tg.[Order]
		, g.[Guid] = tg.[Guid]
		, g.ModifiedDateTime = tg.ModifiedDateTime
		, g.ModifiedByPersonAliasId = tg.ModifiedByPersonAliasId
		, g.ForeignKey = tg.ForeignKey
		, g.ForeignId = tg.ForeignId
		, g.AllowGuests = tg.AllowGuests
		, g.IsPublic = tg.IsPublic
		, g.GroupCapacity = tg.GroupCapacity 
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (IsSystem
		, GroupTypeId
		, ParentGroupId
		, CampusId
		, [Name]
		, [Description]
		, ScheduleId
		, IsSecurityRole
		, IsActive
		, [Order]
		, [Guid]
		, CreatedDateTime
		, CreatedByPersonAliasId
		, ModifiedDateTime
		, ModifiedByPersonAliasId
		, ForeignKey
		, ForeignId
		, AllowGuests
		, IsPublic
		, GroupCapacity)
		VALUES ( tg.IsSystem
		, tg.GroupTypeId
		, tg.ParentGroupId
		, tg.CampusId
		, tg.[Name]
		, tg.[Description]
		, tg.ScheduleId
		, tg.IsSecurityRole
		, tg.IsActive
		, tg.[Order]
		, tg.[Guid]
		, tg.CreatedDateTime
		, tg.CreatedByPersonAliasId
		, tg.ModifiedDateTime
		, tg.ModifiedByPersonAliasId
		, tg.ForeignKey
		, tg.ForeignId
		, tg.AllowGuests
		, tg.IsPublic
		, tg.GroupCapacity );

	/* 
		Update Parent Group Ids
	*/
	Update g 
		Set	ParentGroupId = pg.Id
	From [Group] g
	Join [Group] g1 on g1.Id = g.ForeignId
					And g.ForeignKey = @foreignKey
	Join [Group] g2 on g2.Id = g1.ParentGroupId
	Join [Group] pg on ( pg.ForeignId = g2.Id 
					and pg.ForeignKey = @ForeignKey )
	WHERE pg.Id != g.ParentGroupId

	/* Code to parse the multiple key/value pairs into a table */
	Declare @docHandle int
	exec sp_xml_preparedocument @docHandle output, @roleMap

	/*
		Insert Group Members
	*/
	Insert Into GroupMember (
		IsSystem
		, GroupId
		, PersonId
		, GroupRoleId
		, GroupMemberStatus
		, [Guid]
		, CreatedDateTime
		, CreatedByPersonAliasId
		, ModifiedDateTime
		, ModifiedByPersonAliasId
		, ForeignKey
		, ForeignId
		, IsNotified
		, [Note]
		, IsArchived )
	Select
		IsSystem = m.IsSystem
		, GroupId = g.Id
		, m.PersonId
		, roles.[NewId]
		, m.GroupMemberStatus
		, [Guid] = NEWID()
		, CreatedDateTime = GetDate()
		, CreatedByPersonAliasId = dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
		, ModifiedDateTime = GetDate()
		, ModifiedByPersonAliasId = dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
		, ForeignKey = @ForeignKey
		, ForeignId = m.Id
		, m.IsNotified
		, m.[Note]
		, m.IsArchived
	From
		GroupMember m
		Join [Group] g on g.ForeignKey = @ForeignKey 
					and g.ForeignId = m.GroupId
		LEFT JOIN GroupMember m2 on m2.ForeignKey = @ForeignKey 
					and m2.ForeignId = m.Id
		Left Join GroupMember dup on dup.PersonId = m.PersonId 
		            and dup.GroupId = m2.GroupId 
					and dup.GroupRoleId = m.GroupRoleId
		Inner Join OpenXml(@docHandle, N'/root/Item', 1) 
			With( [OldId] int, [NewId] int) roles on roles.OldId = m.GroupRoleId
	Where m.GroupId in ( Select Id from @GroupIds )
	and m.IsArchived = 0
	and m2.Id is null
	and dup.Id is null

	/*
		INSERT LOCATIONS
	*/
	Insert Into Location (
		IsActive
		, GeoPoint
		, GeoFence
		, Street1
		, Street2
		, City
		, [State]
		, Country
		, StandardizeAttemptedDateTime
		, StandardizeAttemptedServiceType
		, StandardizeAttemptedResult
		, StandardizedDateTime
		, GeocodeAttemptedDateTime
		, GeocodeAttemptedServiceType
		, GeocodeAttemptedResult
		, GeocodedDateTime
		, [Guid]
		, CreatedDateTime
		, CreatedByPersonAliasId
		, ModifiedDateTime
		, ModifiedByPersonAliasId
		, IsGeoPointLocked
		, PostalCode
		, County
		, Barcode
		, ForeignKey
		, ForeignId )
	Select 
		l.IsActive
		, l.GeoPoint
		, l.GeoFence
		, l.Street1
		, l.Street2
		, l.City
		, l.[State]
		, l.Country
		, l.StandardizeAttemptedDateTime
		, l.StandardizeAttemptedServiceType
		, l.StandardizeAttemptedResult
		, l.StandardizedDateTime
		, l.GeocodeAttemptedDateTime
		, l.GeocodeAttemptedServiceType
		, l.GeocodeAttemptedResult
		, l.GeocodedDateTime
		, [Guid] = NEWID()
		, CreatedDateTime = GetDate()
		, CreatedByPersonAliasId = dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
		, ModifiedDateTime = GetDate()
		, ModifiedByPersonAliasId = dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
		, l.IsGeoPointLocked
		, l.PostalCode
		, l.County
		, l.Barcode
		, ForeignKey = @ForeignKey
		, ForeignId = l.Id
	From [Location] l
	LEFT Join [Location] l2 on l2.ForeignKey = @ForeignKey
					AND l2.ForeignId = l.Id
	Where l.Id In (
		Select Distinct 
			LocationId 
		From
			GroupLocation 
		where 
			GroupId in ( Select Id From @GroupIds )
	)
	AND l2.Id is null

	/*
		INSERT GROUP LOCATIONS
	*/
	Insert Into GroupLocation (
		GroupId
		, LocationId
		, GroupLocationTypeValueId
		, IsMailingLocation
		, IsMappedLocation
		, [Guid]
		, CreatedDateTime
		, CreatedByPersonAliasId
		, ModifiedDateTime
		, ModifiedByPersonAliasId
		, GroupMemberPersonAliasId
		, [Order]
		, ForeignKey
		, ForeignId )
	Select 
			GroupId = g.Id
		, LocationId = l.Id
		, gl.GroupLocationTypeValueId
		, gl.IsMailingLocation
		, gl.IsMappedLocation
		, [Guid] = NEWID()
		, CreatedDateTime = GetDate()
		, CreatedByPersonAliasId = dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
		, ModifiedDateTime = GetDate()
		, ModifiedByPersonAliasId = dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
		, gl.GroupMemberPersonAliasId
		, gl.[Order]
		, ForeignKey = @ForeignKey
		, ForeignId	= gl.Id
	From
		GroupLocation gl
	JOIN [Group] g on g.ForeignKey = @ForeignKey
				AND g.ForeignId = gl.GroupId
	JOIN [Location] l on l.ForeignKey = @ForeignKey
				AND l.ForeignId = gl.LocationId
	LEFT JOIN GroupLocation gl2 on gl2.ForeignKey = @ForeignKey
				AND gl2.ForeignId = gl.Id
	
	where gl.GroupId in ( Select Id From @GroupIds )
	AND gl2.Id is null


	/*
		INSERT GROUP LOCATION SCHEDULES
	*/
	Insert Into GroupLocationSchedule(
		GroupLocationId
		, ScheduleId )
	Select 
		gl2.Id, gls.ScheduleId
	From
		GroupLocationSchedule gls
	Inner Join GroupLocation gl on gls.GroupLocationId = gl.Id
	JOIN [Group] g on g.ForeignKey = @ForeignKey
				AND g.ForeignId = gl.GroupId
	JOIN [Location] l on l.ForeignKey = @ForeignKey
				AND l.ForeignId = gl.LocationId
	Inner JOIN GroupLocation gl2 on gl2.GroupId = g.Id
				AND gl2.LocationId = l.Id	
	where gl.GroupId in ( Select Id From @GroupIds )


	/*
		Insert Group Sync Settings
	*/

	INSERT INTO [GroupSync] (
		GroupId
		, GroupTypeRoleId
		, SyncDataViewId
		, WelcomeSystemEmailId
		, ExitSystemEmailId
		, AddUserAccountsDuringSync
		, CreatedDateTime
		, CreatedByPersonAliasId
		, ModifiedDateTime
		, ModifiedByPersonAliasId
		, [Guid]
		, ForeignKey
		, ForeignId )
	Select 
		GroupId = g.Id
		, gs.GroupTypeRoleId
		, gs.SyncDataViewId
		, gs.WelcomeSystemEmailId
		, gs.ExitSystemEmailId
		, gs.AddUserAccountsDuringSync
		, CreatedDateTime = GetDate()
		, CreatedByPersonAliasId = dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
		, ModifiedDateTime = GetDate()
		, ModifiedByPersonAliasId = dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
		, [Guid] = NEWID()
		, ForeignKey = @ForeignKey
		, ForeignId = gs.Id
	From 
		GroupSync gs
	JOIN [Group] g on g.ForeignKey = @ForeignKey
				AND g.ForeignId = gs.GroupId
	LEFT JOIN GroupSync gs2 on gs2.ForeignKey = @ForeignKey
					AND gs2.ForeignId = gs.Id
	Where 
		gs.GroupId in ( Select Id From @GroupIds )
	And gs2.Id is null



	/* Code to parse the multiple key/value pairs into a table */
	Declare @docHandle2 int
	exec sp_xml_preparedocument @docHandle2 output, @attributeMap


	/*
		INSERT Group Attribute Values
	*/
	DECLARE @AttributeValues TABLE( [Guid] uniqueidentifier, IsSystem bit, AttributeId int, EntityId int, [Value] nvarchar(max), CreatedDateTime datetime )

	INSERT INTO @AttributeValues ([Guid], IsSystem, AttributeId, EntityId, [Value], CreatedDateTime)
		SELECT NewId(), 0, attrMap.[NewId], g2.Id, av.[Value], GetDate()
	From
	[Group] g 
	Join AttributeValue av on av.EntityId = g.Id 
	Join [Group] g2 on g2.ForeignKey = @ForeignKey 
					and g2.ForeignId = g.Id
	Inner Join OpenXml(@docHandle2, N'/root/Item', 1) 
			With( [OldId] int, [NewId] int) attrMap on attrMap.OldId = av.AttributeId
	Where g.id in ( Select Id From @GroupIds )


	MERGE AttributeValue AV
		USING @AttributeValues V ON AV.EntityId = V.EntityId AND AV.AttributeId = V.AttributeId
		WHEN MATCHED THEN
			UPDATE SET AV.[Value] = V.[Value], AV.ModifiedDateTime = GetDate(), AV.ModifiedByPersonAliasId = dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
		WHEN NOT MATCHED BY TARGET THEN
			INSERT ([Guid], IsSystem, AttributeId, EntityId, [Value], CreatedDateTime, CreatedByPersonAliasId)
			VALUES ( V.[Guid], V.IsSystem, V.AttributeId, V.EntityId, V.[Value], V.CreatedDateTime, dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId ));

	/* Code to parse the multiple key/value pairs into a table */
	Declare @docHandle3 int
	exec sp_xml_preparedocument @docHandle3 output, @memberAttributeMap


	/*
		INSERT Member Attribute Values
	*/
	DECLARE @MemberAttributeValues TABLE( [Guid] uniqueidentifier, IsSystem bit, AttributeId int, EntityId int, [Value] nvarchar(max), CreatedDateTime datetime )

	INSERT INTO @MemberAttributeValues ([Guid], IsSystem, AttributeId, EntityId, [Value], CreatedDateTime)
		SELECT NewId(), 0, attrMap.[NewId], gm2.Id, av.[Value], GetDate()
	From
	GroupMember gm
	Join AttributeValue av on av.EntityId = gm.Id 
	Join [GroupMember] gm2 on gm2.ForeignKey = @ForeignKey 
					and gm2.ForeignId = gm.Id
	Inner Join OpenXml(@docHandle3, N'/root/Item', 1) 
			With( [OldId] int, [NewId] int) attrMap on attrMap.OldId = av.AttributeId
	Where gm.GroupId in ( Select Id From @GroupIds )


	MERGE AttributeValue AV
		USING @AttributeValues V ON AV.EntityId = V.EntityId AND AV.AttributeId = V.AttributeId
		WHEN MATCHED THEN
			UPDATE SET AV.[Value] = V.[Value], AV.ModifiedDateTime = GetDate(), AV.ModifiedByPersonAliasId = dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
		WHEN NOT MATCHED BY TARGET THEN
			INSERT ([Guid], IsSystem, AttributeId, EntityId, [Value], CreatedDateTime, CreatedByPersonAliasId)
			VALUES ( V.[Guid], V.IsSystem, V.AttributeId, V.EntityId, V.[Value], V.CreatedDateTime, dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId ) );

	if @copyAttendanceYN = 1
	BEGIN
	/* 
		Insert Attendance Occurrence Values
	*/

	DECLARE @AttendanceOccurrence TABLE( [Id] Int
      ,[GroupId] Int
      ,[LocationId] Int
      ,[ScheduleId] Int
      ,[OccurrenceDate] Date
      ,[DidNotOccur] bit
      ,[CreatedDateTime] DateTime
      ,[ModifiedDateTime] DateTime
      ,[CreatedByPersonAliasId] Int
      ,[ModifiedByPersonAliasId] Int
      ,[Guid] uniqueidentifier
      ,[ForeignId] int
      ,[ForeignKey] nvarchar(100)
      ,[Notes] nvarchar(max)
      ,[AnonymousAttendanceCount] int
      ,[StepTypeId] int
      ,[AcceptConfirmationMessage] nvarchar(max)
      ,[DeclineConfirmationMessage] nvarchar(max)
      ,[ShowDeclineReasons] bit
      ,[DeclineReasonValueIds] nvarchar(250)
      ,[SundayDate] date
      ,[Name] nvarchar(250))

	INSERT INTO @AttendanceOccurrence ([Id]
      ,[GroupId]
      ,[LocationId]
      ,[ScheduleId]
      ,[OccurrenceDate]
      ,[DidNotOccur]
      ,[CreatedDateTime]
      ,[ModifiedDateTime]
      ,[CreatedByPersonAliasId]
      ,[ModifiedByPersonAliasId]
      ,[Guid]
      ,[ForeignId]
      ,[ForeignKey]
      ,[Notes]
      ,[AnonymousAttendanceCount]
      ,[StepTypeId]
      ,[AcceptConfirmationMessage]
      ,[DeclineConfirmationMessage]
      ,[ShowDeclineReasons]
      ,[DeclineReasonValueIds]
      ,[SundayDate]
      ,[Name])
		SELECT ao.[Id]
      ,g.Id
      ,ao.[LocationId]
      ,ao.[ScheduleId]
      ,ao.[OccurrenceDate]
      ,ao.[DidNotOccur]
      ,GetDate()
      ,GetDate()
      ,dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
      ,dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
      ,NewId()
      ,ao.[Id]
      ,@ForeignKey
      ,ao.[Notes]
      ,ao.[AnonymousAttendanceCount]
      ,ao.[StepTypeId]
      ,ao.[AcceptConfirmationMessage]
      ,ao.[DeclineConfirmationMessage]
      ,ao.[ShowDeclineReasons]
      ,ao.[DeclineReasonValueIds]
      ,ao.[SundayDate]
      ,ao.[Name]
	From
	AttendanceOccurrence ao
	Inner Join [Group] g on g.ForeignId = ao.GroupId and g.ForeignKey = @ForeignKey
	Where ao.GroupId in ( Select Id From @GroupIds )


	MERGE AttendanceOccurrence AO
		USING @AttendanceOccurrence A ON AO.GroupId = A.GroupId AND ISNULL(AO.LocationId,0) = ISNULL(A.LocationId,0) And ISNULL(AO.ScheduleId,0) = ISNULL(A.ScheduleId,0) And AO.OccurrenceDate = A.OccurrenceDate
		WHEN MATCHED THEN
			UPDATE SET AO.ForeignKey = @ForeignKey, AO.ForeignId = A.ID, AO.ModifiedDateTime = GetDate(), AO.ModifiedByPersonAliasId = dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
		WHEN NOT MATCHED BY TARGET THEN
			INSERT ([GroupId]
      ,[LocationId]
      ,[ScheduleId]
      ,[OccurrenceDate]
      ,[DidNotOccur]
      ,[CreatedDateTime]
      ,[ModifiedDateTime]
      ,[CreatedByPersonAliasId]
      ,[ModifiedByPersonAliasId]
      ,[Guid]
      ,[ForeignId]
      ,[ForeignKey]
      ,[Notes]
      ,[AnonymousAttendanceCount]
      ,[StepTypeId]
      ,[AcceptConfirmationMessage]
      ,[DeclineConfirmationMessage]
      ,[ShowDeclineReasons]
      ,[DeclineReasonValueIds]
      ,[SundayDate]
      ,[Name])
			VALUES ( [GroupId]
      ,[LocationId]
      ,[ScheduleId]
      ,[OccurrenceDate]
      ,[DidNotOccur]
      ,[CreatedDateTime]
      ,[ModifiedDateTime]
      ,[CreatedByPersonAliasId]
      ,[ModifiedByPersonAliasId]
      ,[Guid]
      ,[ForeignId]
      ,[ForeignKey]
      ,[Notes]
      ,[AnonymousAttendanceCount]
      ,[StepTypeId]
      ,[AcceptConfirmationMessage]
      ,[DeclineConfirmationMessage]
      ,[ShowDeclineReasons]
      ,[DeclineReasonValueIds]
      ,[SundayDate]
      ,[Name] );

	/* 
		Insert Attendance Values
	*/
	DECLARE @Attendance TABLE( [Id] int
      ,[DeviceId] int
      ,[SearchTypeValueId] int
      ,[AttendanceCodeId] int
      ,[QualifierValueId] int
      ,[StartDateTime] datetime
      ,[EndDateTime] datetime
      ,[DidAttend] bit
      ,[Note] nvarchar(max)
      ,[Guid] uniqueidentifier
      ,[CreatedDateTime] datetime
      ,[ModifiedDateTime] datetime
      ,[CreatedByPersonAliasId] int
      ,[ModifiedByPersonAliasId] int
      ,[ForeignKey] nvarchar(100)
      ,[CampusId] int
      ,[PersonAliasId] int
      ,[RSVP] int
      ,[Processed] bit
      ,[ForeignId] int
      ,[SearchValue] nvarchar(max)
      ,[SearchResultGroupId] int
      ,[OccurrenceId] int
      ,[CheckedInByPersonAliasId] int
      ,[ScheduledToAttend] bit
      ,[RequestedToAttend] bit
      ,[ScheduleConfirmationSent] bit
      ,[ScheduleReminderSent] bit
      ,[RSVPDateTime] datetime
      ,[DeclineReasonValueId] int
      ,[ScheduledByPersonAliasId] int )

	INSERT INTO @Attendance ([Id]
      ,[DeviceId]
      ,[SearchTypeValueId]
      ,[AttendanceCodeId]
      ,[QualifierValueId]
      ,[StartDateTime]
      ,[EndDateTime]
      ,[DidAttend]
      ,[Note]
      ,[Guid]
      ,[CreatedDateTime]
      ,[ModifiedDateTime]
      ,[CreatedByPersonAliasId]
      ,[ModifiedByPersonAliasId]
      ,[ForeignKey]
      ,[CampusId]
      ,[PersonAliasId]
      ,[RSVP]
      ,[Processed]
      ,[ForeignId]
      ,[SearchValue]
      ,[SearchResultGroupId]
      ,[OccurrenceId]
      ,[CheckedInByPersonAliasId]
      ,[ScheduledToAttend]
      ,[RequestedToAttend]
      ,[ScheduleConfirmationSent]
      ,[ScheduleReminderSent]
      ,[RSVPDateTime]
      ,[DeclineReasonValueId]
      ,[ScheduledByPersonAliasId])
		SELECT a.[Id]
      ,a.[DeviceId]
      ,a.[SearchTypeValueId]
      ,a.[AttendanceCodeId]
      ,a.[QualifierValueId]
      ,a.[StartDateTime]
      ,a.[EndDateTime]
      ,a.[DidAttend]
      ,a.[Note]
      ,NewId()
      ,GetDate()
      ,GetDate()
      ,dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
      ,dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
      ,@ForeignKey
      ,a.[CampusId]
      ,a.[PersonAliasId]
      ,a.[RSVP]
      ,a.[Processed]
      ,a.Id
      ,a.[SearchValue]
      ,a.[SearchResultGroupId]
      ,ao2.Id
      ,a.[CheckedInByPersonAliasId]
      ,a.[ScheduledToAttend]
      ,a.[RequestedToAttend]
      ,a.[ScheduleConfirmationSent]
      ,a.[ScheduleReminderSent]
      ,a.[RSVPDateTime]
      ,a.[DeclineReasonValueId]
      ,a.[ScheduledByPersonAliasId]
	From
	Attendance a
	Inner Join AttendanceOccurrence ao on a.OccurrenceId = ao.Id
	Inner Join AttendanceOccurrence ao2 on ao2.ForeignId = a.OccurrenceId and ao2.ForeignKey = @ForeignKey
	Where ao.GroupId in ( Select Id From @GroupIds )


	MERGE Attendance A
		USING @Attendance A2 ON
            ( A2.OccurrenceId = A.OccurrenceId
            And A2.PersonAliasId = A.PersonAliasId )
		--WHEN MATCHED THEN
		--	UPDATE SET [DeviceId] = A2.DeviceId
  --    ,[SearchTypeValueId] = A2.SearchTypeValueId
  --    ,[AttendanceCodeId] = A2.AttendanceCodeId
  --    ,[QualifierValueId] = A2.QualifierValueId
  --    ,[StartDateTime] = A2.StartDateTime
  --    ,[EndDateTime] = A2.EndDateTime
  --    ,[DidAttend] = A2.DidAttend
  --    ,[Note] = A2.Note
  --    ,[CampusId] = A2.CampusId
  --    ,[RSVP] = A2.RSVP
  --    ,[Processed] = A2.Processed
  --    ,[ForeignId] = A2.ForeignId
  --    ,[SearchValue] = A2.SearchValue
  --    ,[SearchResultGroupId] = A2.SearchResultGroupId
  --    ,[CheckedInByPersonAliasId] = A2.CheckedInByPersonAliasId
  --    ,[ScheduledToAttend] = A2.ScheduledToAttend
  --    ,[RequestedToAttend] = A2.RequestedToAttend
  --    ,[ScheduleConfirmationSent] = A2.ScheduleConfirmationSent
  --    ,[ScheduleReminderSent] = A2.ScheduleReminderSent
  --    ,[RSVPDateTime] = A2.RSVPDateTime
  --    ,[DeclineReasonValueId] = A2.DeclineReasonValueId
  --    ,[ScheduledByPersonAliasId] = A2.ScheduledByPersonAliasId
		--, A.ModifiedDateTime = GetDate(), A.ModifiedByPersonAliasId = dbo.ufnUtility_GetPrimaryPersonAliasId ( @personId )
		WHEN NOT MATCHED BY TARGET THEN
			INSERT ([DeviceId]
      ,[SearchTypeValueId]
      ,[AttendanceCodeId]
      ,[QualifierValueId]
      ,[StartDateTime]
      ,[EndDateTime]
      ,[DidAttend]
      ,[Note]
      ,[Guid]
      ,[CreatedDateTime]
      ,[ModifiedDateTime]
      ,[CreatedByPersonAliasId]
      ,[ModifiedByPersonAliasId]
      ,[ForeignKey]
      ,[CampusId]
      ,[PersonAliasId]
      ,[RSVP]
      ,[Processed]
      ,[ForeignId]
      ,[SearchValue]
      ,[SearchResultGroupId]
      ,[OccurrenceId]
      ,[CheckedInByPersonAliasId]
      ,[ScheduledToAttend]
      ,[RequestedToAttend]
      ,[ScheduleConfirmationSent]
      ,[ScheduleReminderSent]
      ,[RSVPDateTime]
      ,[DeclineReasonValueId]
      ,[ScheduledByPersonAliasId])
			VALUES ([DeviceId]
      ,[SearchTypeValueId]
      ,[AttendanceCodeId]
      ,[QualifierValueId]
      ,[StartDateTime]
      ,[EndDateTime]
      ,[DidAttend]
      ,[Note]
      ,[Guid]
      ,[CreatedDateTime]
      ,[ModifiedDateTime]
      ,[CreatedByPersonAliasId]
      ,[ModifiedByPersonAliasId]
      ,[ForeignKey]
      ,[CampusId]
      ,[PersonAliasId]
      ,[RSVP]
      ,[Processed]
      ,[ForeignId]
      ,[SearchValue]
      ,[SearchResultGroupId]
      ,[OccurrenceId]
      ,[CheckedInByPersonAliasId]
      ,[ScheduledToAttend]
      ,[RequestedToAttend]
      ,[ScheduleConfirmationSent]
      ,[ScheduleReminderSent]
      ,[RSVPDateTime]
      ,[DeclineReasonValueId]
      ,[ScheduledByPersonAliasId] );




	END

    
    /* Remove All GroupMigration ForeignIds from Groups */
    --UPDATE [Group]
    --SET ForeignId = NULL
    --WHERE ForeignKey = @ForeignKey


END
GO



