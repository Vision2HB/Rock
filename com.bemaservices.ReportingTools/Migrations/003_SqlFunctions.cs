// <copyright>
// Copyright by BEMA Information Technologies
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Resources;
using Rock.Plugin;

namespace com.bemaservices.ReportingTools
{
    [MigrationNumber( 3, "1.10.1" )]
    public class SqlFunctions : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            BEMA_ReportingTools_ufn_GetFamilyLastNames();
            BEMA_ReportingTools_ufn_GetFamilyNickNames();
            BEMA_ReportingTools_ufn_GetFamilyTitleFormal();
            BEMA_ReportingTools_ufn_GetFamilyTitles();
            BEMA_ReportingTools_ufn_GetGivingUnitHeadOfHousePersonIdFromGivingId();
            BEMA_ReportingTools_ufn_GetHeadOfHousePersonIdFromPersonId();
            BEMA_ReportingTools_sp_ReportingFieldsDataset();
        }

        private void BEMA_ReportingTools_ufn_GetHeadOfHousePersonIdFromPersonId()
        {
            Sql( @"CREATE FUNCTION [dbo].[BEMA_ReportingTools_ufn_GetHeadOfHousePersonIdFromPersonId](@PersonId int) 

RETURNS int AS
BEGIN
	
	DECLARE @hohId int

	set @hohId = (Select top(1) CASE WHEN MaleID IS NOT NULL THEN MaleID WHEN FemaleID IS NOT NULL THEN FemaleID ELSE UnkownID END
	FROM Person p
		join GroupMember gm on gm.PersonId = p.id 
		join [group] g2 on g2.id = gm.GroupId and g2.GroupTypeId = 10
		outer apply (
				  SELECT 
			G.Id
			, MAX(CASE WHEN p.Gender = 1 
				THEN p.id ELSE NULL END) MaleID
			, MAX(CASE WHEN p.Gender = 2 
				THEN p.id ELSE NULL END) FemaleID
			, MAX(CASE WHEN p.Gender = 0 
				THEN p.id ELSE NULL END) UnkownID
			FROM GroupMember M
				INNER JOIN [Group] G on G.Id = M.GroupId
				INNER JOIN [GroupTypeRole] R on R.id = M.GroupRoleId
				INNER JOIN [Person] P on P.Id = M.PersonId
				LEFT OUTER JOIN AttributeValue avHOH on avHOH.[EntityId] = p.[Id] and avHOH.[AttributeId] = (SELECT att.[Id] FROM Attribute att WHERE att.[Key] = 'HeadofHousehold')
			WHERE
				G.GroupTypeId = 10
				AND R.Name LIKE 'Adult'
				AND p.RecordTypeValueId = 1
				and g.id = g2.id
			GROUP BY G.id
			) as hoh
	WHERE p.id = @PersonID
	)


	RETURN @hohId
END
" );
        }

        private void BEMA_ReportingTools_ufn_GetGivingUnitHeadOfHousePersonIdFromGivingId()
        {
            Sql( @"CREATE FUNCTION [dbo].[BEMA_ReportingTools_ufn_GetGivingUnitHeadOfHousePersonIdFromGivingId](@GivingId varchar(31) ) 

RETURNS int AS
BEGIN
	DECLARE @UnitType char(1)
	DECLARE @UnitId int
	DECLARE @Result int

	SET @UnitType = LEFT(@GivingId, 1)
	SET @UnitId = CAST(SUBSTRING(@GivingId, 2, LEN(@GivingId)) AS INT)

	IF @UnitType = 'P' -- person
		SET @Result = @UnitId
	ELSE -- family
		SET @Result =	(
							SELECT TOP 1 p.[Id] 
							FROM 
								[Person] p
								INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
								INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
							WHERE 
								gtr.[Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- adult
								AND gm.[GroupId] = @UnitId
								AND p.GivingId = @GivingId
							ORDER BY p.[Gender]
						)

	RETURN @Result
END
" );
        }

        private void BEMA_ReportingTools_ufn_GetFamilyTitles()
        {
            Sql( @"CREATE FUNCTION [dbo].[BEMA_ReportingTools_ufn_GetFamilyTitles] (
    @PersonId INT
    ,@GroupId INT
    ,@GroupPersonIds VARCHAR(max) = NULL
    ,@UseNickName BIT = 0
    )
RETURNS @PersonNamesTable TABLE (PersonNames VARCHAR(max))
AS
BEGIN
    DECLARE @PersonNames VARCHAR(max);
    DECLARE @AdultLastNameCount INT;
    DECLARE @GroupFirstOrNickNames VARCHAR(max) = '';
    DECLARE @GroupLastName VARCHAR(max);
    DECLARE @GroupAdultFullNames VARCHAR(max) = '';
    DECLARE @GroupNonAdultFullNames VARCHAR(max) = '';
	DECLARE @GroupTitles VARCHAR(max) = '';
    DECLARE @GroupMemberTable TABLE (
		Title VARCHAR(max)
        ,LastName VARCHAR(max)
        ,FirstOrNickName VARCHAR(max)
        ,FullName VARCHAR(max)
		,FullNameWTitle VARCHAR(max)
        ,Gender INT
        ,GroupRoleGuid UNIQUEIDENTIFIER
        );
    DECLARE @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT UNIQUEIDENTIFIER = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42';

    IF (@PersonId IS NOT NULL)
    BEGIN
        -- just getting the Person Names portion of the address for an individual person
        SELECT @PersonNames = CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END + ' ' + ISNULL([p].[LastName], '') + ' ' + ISNULL([dv].[Value], '')
        FROM [Person] [p]
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
		LEFT OUTER JOIN [DefinedValue] [dvtitle] ON [dvtitle].[Id] = [p].[TitleValueId]
        WHERE [p].[Id] = @PersonId;
    END
    ELSE
    BEGIN
        -- populate a table variable with the data we'll need for the different cases
        -- if GroupPersonIds is set (comma-delimited) only a subset of the family members should be combined
        INSERT INTO @GroupMemberTable
        SELECT 
			[dvtitle].[Value] as [Title]
			,[p].[LastName]
            ,CASE @UseNickName
                WHEN 1 
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END [FirstName]
            ,CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END + ' ' + ISNULL([p].[LastName], '') + ' ' + ISNULL([dv].[Value], '') [FullName]
			 ,CASE @UseNickName
                WHEN 1
                    THEN [dvtitle].[Value] + ' ' + ISNULL([p].[NickName], '')
                ELSE [dvtitle].[Value] + ' ' + ISNULL([p].[FirstName], '')
                END + ' ' + ISNULL([p].[LastName], '') + ' ' + ISNULL([dv].[Value], '') [FullNameWTitle]
            ,[p].Gender
            ,[gr].[Guid]
        FROM [GroupMember] [gm]
        JOIN [Person] [p] ON [p].[Id] = [gm].[PersonId]
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
		LEFT OUTER JOIN [DefinedValue] [dvtitle] ON [dvtitle].[Id] = [p].[TitleValueId]
        JOIN [GroupTypeRole] [gr] ON [gm].[GroupRoleId] = [gr].[Id]
        WHERE [GroupId] = @GroupId
            AND (
                ISNULL(@GroupPersonIds, '') = ''
                OR (
                    p.[Id] IN (
                        SELECT *
                        FROM ufnUtility_CsvToTable(@GroupPersonIds)
                        )
                    )
                )

        -- determine adultCount and if we can use the same lastname for all adults, and get lastname while we are at it
        SELECT @AdultLastNameCount = count(DISTINCT [LastName])
            ,@GroupLastName = max([LastName])
        FROM @GroupMemberTable
        WHERE [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT;

        IF @AdultLastNameCount > 0
        BEGIN

			
			SELECT @GroupTitles = @GroupTitles + case when Title is not null then Title + ' & ' else '' end
            FROM @GroupMemberTable g
            WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
            ORDER BY g.Gender 
                ,g.FirstOrNickName
			
			IF @GroupTitles is not null
			BEGIN
            -- get the FirstNames and Adult FullNames for use in the cases of families with Adults
            SELECT @GroupFirstOrNickNames = [FirstOrNickName] + ' & '
            FROM @GroupMemberTable g
            WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
            ORDER BY g.Gender desc
                ,g.FirstOrNickName

			SELECT @GroupAdultFullNames =  @GroupAdultFullNames + [FullNameWTitle] + '& '
            FROM @GroupMemberTable g
            WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
            ORDER BY g.Gender 
                ,g.FirstOrNickName
			END
			ELSE
			SELECT @GroupFirstOrNickNames = @GroupFirstOrNickNames + [FirstOrNickName] + ' & '
                ,@GroupAdultFullNames = @GroupAdultFullNames + [FullName] + '& '
            FROM @GroupMemberTable g
            WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
            ORDER BY g.Gender
                ,g.FirstOrNickName

            -- cleanup the trailing ' &'s
            IF len(@GroupFirstOrNickNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupFirstOrNickNames = SUBSTRING(@GroupFirstOrNickNames, 0, len(@GroupFirstOrNickNames) - 1)
            END

            IF len(@GroupAdultFullNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupAdultFullNames = SUBSTRING(@GroupAdultFullNames, 0, len(@GroupAdultFullNames) - 1)
            END

			  -- cleanup the trailing ' &'s
            IF len(@GroupTitles) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupTitles = SUBSTRING(@GroupTitles, 0, len(@GroupTitles) - 1)
            END

            -- if all the firstnames are blanks, get rid of the '&'
            IF (LTRIM(RTRIM(@GroupFirstOrNickNames)) = '&')
            BEGIN
                SET @GroupFirstOrNickNames = ''
            END

            -- if all the fullnames are blanks, get rid of the '&'
            IF (LTRIM(RTRIM(@GroupAdultFullNames)) = '&')
            BEGIN
                SET @GroupAdultFullNames = ''
            END
        END

        IF @AdultLastNameCount = 0
        BEGIN
            -- get the NonAdultFullNames for use in the case of families without adults 
            SELECT @GroupNonAdultFullNames = @groupNonAdultFullNames + [FullName] + ' & '
            FROM @GroupMemberTable
            ORDER BY [FullName]

            IF len(@GroupNonAdultFullNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupNonAdultFullNames = SUBSTRING(@GroupNonAdultFullNames, 0, len(@GroupNonAdultFullNames) - 1)
            END

            -- if all the fullnames are blanks, get rid of the '&'
            IF (LTRIM(RTRIM(@GroupNonAdultFullNames)) = '&')
            BEGIN
                SET @GroupNonAdultFullNames = ''
            END
        END

        IF (@AdultLastNameCount = 1)
        BEGIN
            -- just one lastname and at least one adult. Get the Person Names portion of the address in the format <MaleAdult> & <FemaleAdult> <LastName>
			IF (@GroupTitles is not null)
            SET @PersonNames = @GroupTitles;
			ELSE 
			SET @PersonNames = ''
        END
        ELSE IF (@AdultLastNameCount = 0)
        BEGIN
            -- no adults in family, list all members of the family in 'Fullname & FullName & ...' format
            SET @PersonNames = @GroupTitles;
        END
        ELSE
        BEGIN
            -- multiple adult lastnames
            SET @PersonNames = @GroupTitles;
        END
    END

    --WHILE (len(@PersonNames) - len(replace(@PersonNames, ' & ', '  ')) > 1)
    --BEGIN
    --    SET @PersonNames = Stuff(@PersonNames, CharIndex(' & ', @PersonNames), Len(' & '), ', ')
    --END

    INSERT INTO @PersonNamesTable ([PersonNames])
    VALUES (@PersonNames);

    RETURN
END
" );
        }

        private void BEMA_ReportingTools_ufn_GetFamilyTitleFormal()
        {
            Sql( @"CREATE FUNCTION [dbo].[BEMA_ReportingTools_ufn_GetFamilyTitleFormal] (
    @PersonId INT
    ,@GroupId INT
    ,@GroupPersonIds VARCHAR(max) = NULL
    ,@UseNickName BIT = 0
    )
RETURNS @PersonNamesTable TABLE (PersonNames VARCHAR(max))
AS
BEGIN
    DECLARE @PersonNames VARCHAR(max);
    DECLARE @AdultLastNameCount INT;
	DECLARE @GroupMemberCount INT;
    DECLARE @GroupFirstOrNickNames VARCHAR(max) = '';
    DECLARE @GroupLastName VARCHAR(max);
    DECLARE @GroupAdultFullNames VARCHAR(max) = '';
    DECLARE @GroupNonAdultFullNames VARCHAR(max) = '';
	DECLARE @GroupTitles VARCHAR(max) = '';
    DECLARE @GroupMemberTable TABLE (
		Title VARCHAR(max)
        ,LastName VARCHAR(max)
        ,FirstOrNickName VARCHAR(max)
        ,FullName VARCHAR(max)
		,FullNameWTitle VARCHAR(max)
        ,Gender INT
        ,GroupRoleGuid UNIQUEIDENTIFIER
        );
    DECLARE @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT UNIQUEIDENTIFIER = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42';

    IF (@PersonId IS NOT NULL)
    BEGIN
        -- just getting the Person Names portion of the address for an individual person
        SELECT @PersonNames = CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END + ' ' + ISNULL([p].[LastName], '') + ' ' + ISNULL([dv].[Value], '')
        FROM [Person] [p]
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
		LEFT OUTER JOIN [DefinedValue] [dvtitle] ON [dvtitle].[Id] = [p].[TitleValueId]
        WHERE [p].[Id] = @PersonId;
    END
    ELSE
    BEGIN
        -- populate a table variable with the data we'll need for the different cases
        -- if GroupPersonIds is set (comma-delimited) only a subset of the family members should be combined
        INSERT INTO @GroupMemberTable
        SELECT 
			[dvtitle].[Value] as [Title]
			,[p].[LastName]
            ,CASE @UseNickName
                WHEN 1 
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END [FirstName]
            ,CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END + ' ' + ISNULL([p].[LastName], '') + ' ' + ISNULL([dv].[Value], '') [FullName]
			 ,CASE @UseNickName
                WHEN 1
                    THEN case when dvtitle.value is not null then [dvtitle].[Value] + ' ' else '' end + ' ' + ISNULL([p].[NickName], '')
                ELSE case when dvtitle.value is not null then [dvtitle].[Value] + ' ' else '' end + ' ' + ISNULL([p].[FirstName], '')
                END + ' ' + ISNULL([p].[LastName], '') + ' ' + ISNULL([dv].[Value], '') [FullNameWTitle]
            ,[p].Gender
            ,[gr].[Guid]
        FROM [GroupMember] [gm]
        JOIN [Person] [p] ON [p].[Id] = [gm].[PersonId]
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
		LEFT OUTER JOIN [DefinedValue] [dvtitle] ON [dvtitle].[Id] = [p].[TitleValueId]
        JOIN [GroupTypeRole] [gr] ON [gm].[GroupRoleId] = [gr].[Id]
        WHERE [GroupId] = @GroupId
            AND (
                ISNULL(@GroupPersonIds, '') = ''
                OR (
                    p.[Id] IN (
                        SELECT *
                        FROM ufnUtility_CsvToTable(@GroupPersonIds)
                        )
                    )
                )

		SET @GroupMemberCount = (Select Count(0) From @GroupMemberTable)

        -- determine adultCount and if we can use the same lastname for all adults, and get lastname while we are at it
        SELECT @AdultLastNameCount = count(DISTINCT [LastName])
            ,@GroupLastName = max([LastName])
        FROM @GroupMemberTable
        WHERE [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT;

        IF @AdultLastNameCount > 0
        BEGIN

			
			SELECT	 @GroupTitles = @GroupTitles + 
									case when Title is not null then Title +' ' else '' end +
									FirstOrNickName+' & '
            FROM @GroupMemberTable g
            WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
            ORDER BY g.Gender 
                ,g.FirstOrNickName
			
			IF @GroupTitles is not null
			BEGIN
            -- get the FirstNames and Adult FullNames for use in the cases of families with Adults
            SELECT @GroupFirstOrNickNames = [FirstOrNickName] + ' & '
            FROM @GroupMemberTable g
            WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
            ORDER BY g.Gender desc
                ,g.FirstOrNickName

			SELECT @GroupAdultFullNames =  @GroupAdultFullNames + IsNull([FullNameWTitle], FullName) + ' & '
            FROM @GroupMemberTable g
            WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
            ORDER BY g.Gender 
                ,g.FirstOrNickName
			END
			ELSE
			SELECT @GroupFirstOrNickNames = @GroupFirstOrNickNames + [FirstOrNickName] + ' & '
                ,@GroupAdultFullNames = @GroupAdultFullNames + [FullName] + '& '
            FROM @GroupMemberTable g
            WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
            ORDER BY g.Gender
                ,g.FirstOrNickName

            -- cleanup the trailing ' &'s
            IF len(@GroupFirstOrNickNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupFirstOrNickNames = SUBSTRING(@GroupFirstOrNickNames, 0, len(@GroupFirstOrNickNames) - 1)
            END

            IF len(@GroupAdultFullNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupAdultFullNames = SUBSTRING(@GroupAdultFullNames, 0, len(@GroupAdultFullNames) - 1)
            END

			  -- cleanup the trailing ' &'s
            IF len(@GroupTitles) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupTitles = SUBSTRING(@GroupTitles, 0, len(@GroupTitles) - 1)
            END

            -- if all the firstnames are blanks, get rid of the '&'
            IF (LTRIM(RTRIM(@GroupFirstOrNickNames)) = '&')
            BEGIN
                SET @GroupFirstOrNickNames = ''
            END

            -- if all the fullnames are blanks, get rid of the '&'
            IF (LTRIM(RTRIM(@GroupAdultFullNames)) = '&')
            BEGIN
                SET @GroupAdultFullNames = ''
            END
        END

		IF @GroupMemberCount = 1
		Begin
			SET @PersonNames = @GroupAdultFullNames;
		End
		Else
		begin
			IF @AdultLastNameCount = 0
			BEGIN
				-- get the NonAdultFullNames for use in the case of families without adults 
				SELECT @GroupNonAdultFullNames = @groupNonAdultFullNames + [FullName] + ' & '
				FROM @GroupMemberTable
				ORDER BY [FullName]

				IF len(@GroupNonAdultFullNames) > 2
				BEGIN
					-- trim the extra ' &' off the end 
					SET @GroupNonAdultFullNames = SUBSTRING(@GroupNonAdultFullNames, 0, len(@GroupNonAdultFullNames) - 1)
				END

				-- if all the fullnames are blanks, get rid of the '&'
				IF (LTRIM(RTRIM(@GroupNonAdultFullNames)) = '&')
				BEGIN
					SET @GroupNonAdultFullNames = ''
				END
			END

			IF (@AdultLastNameCount = 1)
			BEGIN
				-- just one lastname and at least one adult. Get the Person Names portion of the address in the format <MaleAdult> & <FemaleAdult> <LastName>
				IF (@GroupTitles is not null)
				SET @PersonNames = @GroupTitles + ' ' + @GroupLastName;
				ELSE 
				SET @PersonNames = @GroupFirstOrNickNames + ' ' + @GroupLastName;
			END
			ELSE IF (@AdultLastNameCount = 0)
			BEGIN
				-- no adults in family, list all members of the family in 'Fullname & FullName & ...' format
				SET @PersonNames = @GroupNonAdultFullNames;
			END
			ELSE
			BEGIN
				-- multiple adult lastnames
				SET @PersonNames = @GroupAdultFullNames;
			END
		END
    END

    --WHILE (len(@PersonNames) - len(replace(@PersonNames, ' & ', '  ')) > 1)
    --BEGIN
    --    SET @PersonNames = Stuff(@PersonNames, CharIndex(' & ', @PersonNames), Len(' & '), ', ')
    --END

    INSERT INTO @PersonNamesTable ([PersonNames])
    VALUES (LTrim(RTrim(Replace(@PersonNames,'  ',' '))));

    RETURN
END" );
        }

        private void BEMA_ReportingTools_ufn_GetFamilyNickNames()
        {
            Sql( @"
CREATE FUNCTION [dbo].[BEMA_ReportingTools_ufn_GetFamilyNickNames] (
    @PersonId INT
    ,@GroupId INT
    ,@GroupPersonIds VARCHAR(max) = NULL
    ,@UseNickName BIT = 0
    )
RETURNS @PersonNamesTable TABLE (PersonNames VARCHAR(max))
AS
BEGIN
    DECLARE @PersonNames VARCHAR(max);
    DECLARE @AdultLastNameCount INT;
    DECLARE @GroupFirstOrNickNames VARCHAR(max) = '';
    DECLARE @GroupLastName VARCHAR(max);
    DECLARE @GroupAdultFullNames VARCHAR(max) = '';
    DECLARE @GroupNonAdultFullNames VARCHAR(max) = '';
    DECLARE @GroupMemberTable TABLE (
        LastName VARCHAR(max)
        ,FirstOrNickName VARCHAR(max)
        ,FullName VARCHAR(max)
        ,Gender INT
        ,GroupRoleGuid UNIQUEIDENTIFIER
        );
    DECLARE @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT UNIQUEIDENTIFIER = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42';

    IF (@PersonId IS NOT NULL)
    BEGIN
        -- just getting the Person Names portion of the address for an individual person
        SELECT @PersonNames = CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END + ' ' + ISNULL([p].[LastName], '') + ' ' + ISNULL([dv].[Value], '')
        FROM [Person] [p]
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
        WHERE [p].[Id] = @PersonId;
    END
    ELSE
    BEGIN
        -- populate a table variable with the data we'll need for the different cases
        -- if GroupPersonIds is set (comma-delimited) only a subset of the family members should be combined
        INSERT INTO @GroupMemberTable
        SELECT [p].[LastName]
            ,CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END [FirstName]
            ,CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END  [FullName]
            ,[p].Gender
            ,[gr].[Guid]
        FROM [GroupMember] [gm]
        JOIN [Person] [p] ON [p].[Id] = [gm].[PersonId]
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
        JOIN [GroupTypeRole] [gr] ON [gm].[GroupRoleId] = [gr].[Id]
        WHERE [GroupId] = @GroupId
            AND (
                ISNULL(@GroupPersonIds, '') = ''
                OR (
                    p.[Id] IN (
                        SELECT *
                        FROM ufnUtility_CsvToTable(@GroupPersonIds)
                        )
                    )
                )

        -- determine adultCount and if we can use the same lastname for all adults, and get lastname while we are at it
        SELECT @AdultLastNameCount = count(DISTINCT [LastName])
            ,@GroupLastName = max([LastName])
        FROM @GroupMemberTable
        WHERE [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT;

        IF @AdultLastNameCount > 0
        BEGIN
            -- get the FirstNames and Adult FullNames for use in the cases of families with Adults
            SELECT @GroupFirstOrNickNames = @GroupFirstOrNickNames + [FirstOrNickName] + ' & '
                ,@GroupAdultFullNames = @GroupAdultFullNames + [FullName] + ' & '
            FROM @GroupMemberTable g
            WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
            ORDER BY g.Gender
                ,g.FirstOrNickName

            -- cleanup the trailing ' &'s
            IF len(@GroupFirstOrNickNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupFirstOrNickNames = SUBSTRING(@GroupFirstOrNickNames, 0, len(@GroupFirstOrNickNames) - 1)
            END

            IF len(@GroupAdultFullNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupAdultFullNames = SUBSTRING(@GroupAdultFullNames, 0, len(@GroupAdultFullNames) - 1)
            END

            -- if all the firstnames are blanks, get rid of the '&'
            IF (LTRIM(RTRIM(@GroupFirstOrNickNames)) = '&')
            BEGIN
                SET @GroupFirstOrNickNames = ''
            END

            -- if all the fullnames are blanks, get rid of the '&'
            IF (LTRIM(RTRIM(@GroupAdultFullNames)) = '&')
            BEGIN
                SET @GroupAdultFullNames = ''
            END
        END

        IF @AdultLastNameCount = 0
        BEGIN
            -- get the NonAdultFullNames for use in the case of families without adults 
            SELECT @GroupNonAdultFullNames = @groupNonAdultFullNames + [FullName] + ' & '
            FROM @GroupMemberTable
            ORDER BY [FullName]

            IF len(@GroupNonAdultFullNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupNonAdultFullNames = SUBSTRING(@GroupNonAdultFullNames, 0, len(@GroupNonAdultFullNames) - 1)
            END

            -- if all the fullnames are blanks, get rid of the '&'
            IF (LTRIM(RTRIM(@GroupNonAdultFullNames)) = '&')
            BEGIN
                SET @GroupNonAdultFullNames = ''
            END
        END

        IF (@AdultLastNameCount = 1)
        BEGIN
            -- just one lastname and at least one adult. Get the Person Names portion of the address in the format <MaleAdult> & <FemaleAdult> <LastName>
            SET @PersonNames = @GroupFirstOrNickNames;
        END
        ELSE IF (@AdultLastNameCount = 0)
        BEGIN
            -- no adults in family, list all members of the family in 'Fullname & FullName & ...' format
            SET @PersonNames = @GroupNonAdultFullNames;
        END
        ELSE
        BEGIN
            -- multiple adult lastnames
            SET @PersonNames = @GroupAdultFullNames;
        END
    END

    WHILE (len(@PersonNames) - len(replace(@PersonNames, ' & ', '  ')) > 1)
    BEGIN
        SET @PersonNames = Stuff(@PersonNames, CharIndex(' & ', @PersonNames), Len(' & '), ', ')
    END

    INSERT INTO @PersonNamesTable ([PersonNames])
    VALUES (@PersonNames);

    RETURN
END" );
        }

        private void BEMA_ReportingTools_ufn_GetFamilyLastNames()
        {
            Sql( @"CREATE FUNCTION [dbo].[BEMA_ReportingTools_ufn_GetFamilyLastNames] (
    @PersonId INT
    ,@GroupId INT
    ,@GroupPersonIds VARCHAR(max) = NULL
    ,@UseNickName BIT = 0
    )
RETURNS @PersonNamesTable TABLE (PersonNames VARCHAR(max))
AS
BEGIN
    DECLARE @PersonNames VARCHAR(max);
    DECLARE @AdultLastNameCount INT;
    DECLARE @GroupFirstOrNickNames VARCHAR(max) = '';
    DECLARE @GroupLastName VARCHAR(max);
    DECLARE @GroupAdultFullNames VARCHAR(max) = '';
    DECLARE @GroupNonAdultFullNames VARCHAR(max) = '';
    DECLARE @GroupMemberTable TABLE (
        LastName VARCHAR(max)
        ,FirstOrNickName VARCHAR(max)
        ,FullName VARCHAR(max)
        ,Gender INT
        ,GroupRoleGuid UNIQUEIDENTIFIER
        );
    DECLARE @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT UNIQUEIDENTIFIER = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42';

    IF (@PersonId IS NOT NULL)
    BEGIN
        -- just getting the Person Names portion of the address for an individual person
        SELECT @PersonNames = CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END + ' ' + ISNULL([p].[LastName], '') + ' ' + ISNULL([dv].[Value], '')
        FROM [Person] [p]
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
        WHERE [p].[Id] = @PersonId;
    END
    ELSE
    BEGIN
        -- populate a table variable with the data we'll need for the different cases
        -- if GroupPersonIds is set (comma-delimited) only a subset of the family members should be combined
        INSERT INTO @GroupMemberTable
        SELECT [p].[LastName]
            ,CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END [FirstName]
            ,ISNULL([p].[LastName], '') + ' ' [FullName]
            ,[p].Gender
            ,[gr].[Guid]
        FROM [GroupMember] [gm]
        JOIN [Person] [p] ON [p].[Id] = [gm].[PersonId]
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
        JOIN [GroupTypeRole] [gr] ON [gm].[GroupRoleId] = [gr].[Id]
        WHERE [GroupId] = @GroupId
            AND (
                ISNULL(@GroupPersonIds, '') = ''
                OR (
                    p.[Id] IN (
                        SELECT *
                        FROM ufnUtility_CsvToTable(@GroupPersonIds)
                        )
                    )
                )

        -- determine adultCount and if we can use the same lastname for all adults, and get lastname while we are at it
        SELECT @AdultLastNameCount = count(DISTINCT [LastName])
            ,@GroupLastName = max([LastName])
        FROM @GroupMemberTable
        WHERE [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT;

        IF @AdultLastNameCount > 0
        BEGIN
            -- get the FirstNames and Adult FullNames for use in the cases of families with Adults
            SELECT @GroupFirstOrNickNames = @GroupFirstOrNickNames + [FirstOrNickName] + ' & '
                ,@GroupAdultFullNames = @GroupAdultFullNames + [FullName] + '& '
            FROM @GroupMemberTable g
            WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
            ORDER BY g.Gender
                ,g.FirstOrNickName

            -- cleanup the trailing ' &'s
            IF len(@GroupFirstOrNickNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupFirstOrNickNames = SUBSTRING(@GroupFirstOrNickNames, 0, len(@GroupFirstOrNickNames) - 1)
            END

            IF len(@GroupAdultFullNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupAdultFullNames = SUBSTRING(@GroupAdultFullNames, 0, len(@GroupAdultFullNames) - 1)
            END

            -- if all the firstnames are blanks, get rid of the '&'
            IF (LTRIM(RTRIM(@GroupFirstOrNickNames)) = '&')
            BEGIN
                SET @GroupFirstOrNickNames = ''
            END

            -- if all the fullnames are blanks, get rid of the '&'
            IF (LTRIM(RTRIM(@GroupAdultFullNames)) = '&')
            BEGIN
                SET @GroupAdultFullNames = ''
            END
        END

        IF @AdultLastNameCount = 0
        BEGIN
            -- get the NonAdultFullNames for use in the case of families without adults 
            SELECT @GroupNonAdultFullNames = @groupNonAdultFullNames + [FullName] + ' & '
            FROM @GroupMemberTable
            ORDER BY [FullName]

            IF len(@GroupNonAdultFullNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupNonAdultFullNames = SUBSTRING(@GroupNonAdultFullNames, 0, len(@GroupNonAdultFullNames) - 1)
            END

            -- if all the fullnames are blanks, get rid of the '&'
            IF (LTRIM(RTRIM(@GroupNonAdultFullNames)) = '&')
            BEGIN
                SET @GroupNonAdultFullNames = ''
            END
        END

        IF (@AdultLastNameCount = 1)
        BEGIN
            -- just one lastname and at least one adult. Get the Person Names portion of the address in the format <MaleAdult> & <FemaleAdult> <LastName>
            SET @PersonNames =  @GroupLastName;
        END
        ELSE IF (@AdultLastNameCount = 0)
        BEGIN
            -- no adults in family, list all members of the family in 'Fullname & FullName & ...' format
            SET @PersonNames = @GroupNonAdultFullNames;
        END
        ELSE
        BEGIN
            -- multiple adult lastnames
            SET @PersonNames = @GroupAdultFullNames;
        END
    END

    WHILE (len(@PersonNames) - len(replace(@PersonNames, ' & ', '  ')) > 1)
    BEGIN
        SET @PersonNames = Stuff(@PersonNames, CharIndex(' & ', @PersonNames), Len(' & '), ', ')
    END

    INSERT INTO @PersonNamesTable ([PersonNames])
    VALUES (@PersonNames);

    RETURN
END" );
        }

        private void BEMA_ReportingTools_sp_ReportingFieldsDataset()
        {
            Sql( @"
CREATE PROCEDURE [dbo].[BEMA_ReportingTools_sp_ReportingFieldsDataset]
AS
BEGIN
    
    DECLARE @cACTIVE_RECORD_STATUS_VALUE_GUID UNIQUEIDENTIFIER = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
    DECLARE @cPERSON_RECORD_TYPE_VALUE_GUID UNIQUEIDENTIFIER = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
    DECLARE @cFamilyGROUPTYPE_GUID UNIQUEIDENTIFIER = '790E3215-3B10-442B-AF69-616C0DCB998E'
    DECLARE @cADULT_ROLE_GUID UNIQUEIDENTIFIER = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'
    DECLARE @cTRANSACTION_TYPE_CONTRIBUTION UNIQUEIDENTIFIER = '2D607262-52D6-4724-910D-5C6E8FB89ACC';

	Declare @cUPDATE_DATE_ATTRIBUTE_GUID UNIQUEIDENTIFIER = 'A1263027-17BB-433B-A59E-2DF7F821BCD4'

    DECLARE @PersonRecordTypeValueId INT = (
            SELECT TOP 1 [Id]
            FROM [DefinedValue]
            WHERE [Guid] = @cPERSON_RECORD_TYPE_VALUE_GUID
            )
    DECLARE @FamilyGroupTypeId INT = (
            SELECT TOP 1 [Id]
            FROM [GroupType]
            WHERE [Guid] = @cFamilyGROUPTYPE_GUID
            )
    DECLARE @AdultRoleId INT = (
            SELECT TOP 1 [Id]
            FROM [GroupTypeRole]
            WHERE [Guid] = @cADULT_ROLE_GUID
            )
    DECLARE @ContributionType INT = (
            SELECT TOP 1 [Id]
            FROM [DefinedValue]
            WHERE [Guid] = @cTRANSACTION_TYPE_CONTRIBUTION
            )
    DECLARE @UpdateDateAttributeId INT = (
            SELECT TOP 1 [Id]
            FROM Attribute
            WHERE [Guid] = @cUPDATE_DATE_ATTRIBUTE_GUID
            )

    Declare @ModifiedPersonIds table(
			PersonId int,
			GroupId int null,
			GivingGroupPersonIds nvarchar(max)
			)

	Insert into @ModifiedPersonIds
	Select p.Id, 
			familyMembers.GroupId,
			--(Select String_agg(p1.Id,',') From Person p1 where p1.GivingId = p.GivingId) -- SQL SERVER 2017+ Version
			STUFF
			  (
				(
				  SELECT ',' + convert(nvarchar(max),p1.Id)
					FROM Person p1
					WHERE p1.GivingId = p.GivingId
					FOR XML PATH('')
				), 1, 1, N''
			  )
	From Person modifiedPerson
	Left Join (
			Select fm1.PersonId as modifiedFamilyMemberId, 
					g.Id as GroupId,
					fm2.PersonId as familyMemberId
			From GroupMember fm1
			Join [Group] g on g.Id = fm1.GroupId and g.GroupTypeId = @FamilyGroupTypeId and g.IsActive = 1 and g.IsArchived = 0
			Join GroupMember fm2 on fm2.GroupId = g.Id and fm2.GroupMemberStatus = 1
			Join GroupTypeRole gtr on fm2.GroupRoleId = gtr.Id
			Where gtr.Id = @AdultRoleId
		) familyMembers on familyMembers.modifiedFamilyMemberId = modifiedPerson.Id
	Join Person p on p.Id = familyMembers.familyMemberId
	Left Join AttributeValue updateDate on updateDate.EntityId = p.Id and updateDate.AttributeId = @UpdateDateAttributeId
	Where (updateDate.ValueAsDateTime is null or modifiedPerson.ModifiedDateTime > updateDate.ValueAsDateTime)

	Select p.Id,
			mpId.GroupId,
			p.FirstName,
			p.NickName,
			p.LastName,
			case when p.Id = [dbo].[BEMA_ReportingTools_ufn_GetHeadOfHousePersonIdFromPersonId](p.id) then 'True' else 'False' end 
				as FamilyHeadofHousehold,
			case when p.Id = [dbo].[BEMA_ReportingTools_ufn_GetGivingUnitHeadOfHousePersonIdFromGivingId](p.GivingId) then 'True' else 'False' end 
				as GivingUnitHeadofHousehold,
			(SELECT * FROM dbo.ufnCrm_GetFamilyTitle( null, mpId.GroupId, default, 1)) 
				as FamilyFullNameNickNameNoTitle,
			(SELECT * FROM dbo.ufnCrm_GetFamilyTitle( null, mpId.GroupId,mpId.GivingGroupPersonIds , 1) )
				as GivingUnitFullNameNickNameNoTitle,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyNickNames](null, mpId.GroupId, default, 0))
				as FamilyFirstName,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyNickNames]( null, mpId.GroupId, mpId.GivingGroupPersonIds, 0) )
				as GivingUnitFirstName,
			(SELECT * FROM dbo.BEMA_ReportingTools_ufn_GetFamilyTitleFormal(null, mpId.GroupId, default, 1)) 
				as FamilyFullNameNickName,
			(SELECT * FROM dbo.BEMA_ReportingTools_ufn_GetFamilyTitleFormal( null, mpId.GroupId, mpId.GivingGroupPersonIds, 1) )
				as GivingUnitFullNameNickName,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyLastNames](null, mpId.GroupId, default, 1))
				as FamilyLastNames,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyLastNames](null, mpId.GroupId, mpId.GivingGroupPersonIds, 1))
				as GivingUnitLastNames,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyNickNames](null, mpId.GroupId, default, 1))
				as FamilyNickNames,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyNickNames](null, mpId.GroupId, mpId.GivingGroupPersonIds, 1))
				as GivingUnitNickNames,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyTitles](null, mpId.GroupId, default, 1)) 
				as FamilyTitles,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyTitles](null, mpId.GroupId, mpId.GivingGroupPersonIds, 1)) 
				as GivingUnitTitles,
			(SELECT * FROM dbo.BEMA_ReportingTools_ufn_GetFamilyTitleFormal(null, mpId.GroupId, default, 0)) 
				as FamilyFullNameFirstName,
			(SELECT * FROM dbo.BEMA_ReportingTools_ufn_GetFamilyTitleFormal(null, mpId.GroupId, mpId.GivingGroupPersonIds, 0)) 
				as GivingUnitFullNameFirstName
	From Person p
	Join (Select distinct * From @ModifiedPersonIds) mpId on p.Id = mpId.PersonId


END" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}

