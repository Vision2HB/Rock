SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
<doc>
	<summary>
		This stored procedure updates the group type, while creating/updating group roles (not attributes).  
    </summary>

	<param name = 'RootGroupId' datatype='Int'>The Group Tree you'd like to Duplicate</param><param name = 'NewGroupTypeId' datatype='Int'>The new group type</param>
	<param name = 'NewParentGroupId' datatype='Int'>The parent group id to place new groups under</param>
</doc>
*/
CREATE PROCEDURE [dbo].[_com_bemaservices_spChangeGroupType]
(
    @rootGroupId int
	, @newGroupTypeId int
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON


    DECLARE @GroupIds TABLE ( Id int )

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

    --
    -- Update Group Type Id
    --

    UPDATE G 
    SET G.GroupTypeId = @newGroupTypeId
    FROM [Group] G 
    WHERE G.Id IN ( Select Id FROM @GroupIds )

    --
    -- Update Group Roles to new group type roles ( add new ones if necessary )
    --

    DECLARE @GroupRoleNames TABLE( [Name] nvarchar(200) )

    INSERT INTO @GroupRoleNames
    SELECT DISTINCT GTR.[Name] 
    FROM GroupTypeRole GTR 
    INNER JOIN GroupMember GM ON GM.GroupRoleId = GTR.Id
    WHERE GM.GroupId IN ( Select Id FROM @GroupIds )

    -- Capture current new group type roles
    DECLARE @GroupRoleIds TABLE( Id int )

    INSERT INTO @GroupRoleIds
    SELECT GTR.Id 
    FROM GroupTypeRole GTR 
    WHERE GTR.GroupTypeId = @newGroupTypeId

    -- Create new roles, based on name
    MERGE GroupTypeRole GTR 
    USING @GroupRoleNames N ON GTR.[Name] = N.[Name] AND GTR.Id IN ( Select Id From @GroupRoleIds )
    WHEN NOT MATCHED
        THEN INSERT ( IsSystem, GroupTypeId, [Name], IsLeader, [Guid], CreatedDateTime, ModifiedDateTime, CanView, CanEdit, ReceiveRequirementsNotifications, CanManageMembers )
        VALUES ( 0, @newGroupTypeId, N.[Name], 0, NewId(), GetDate(), GetDate(), 0, 0, 0, 0 )
    ;

    DECLARE @NewGroupTypeRoles TABLE ( Id int, [Name] nvarchar(200) )

    INSERT INTO @NewGroupTypeRoles
    SELECT GTR.Id, GTR.[Name]
    FROM GroupTypeRole GTR 
    WHERE GTR.GroupTypeId = @newGroupTypeId

   -- Change Roles Over, Based on Name

   UPDATE GM 
   SET GM.GroupRoleId = ( SELECT TOP 1 Id FROM @NewGroupTypeRoles WHERE [Name] = GTR.[Name] )
   FROM GroupMember GM
   INNER JOIN GroupTypeRole GTR ON GM.GroupRoleId = GTR.Id
   WHERE GM.GroupId IN ( Select Id FROM @GroupIds )

END

GO
