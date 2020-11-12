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
    [MigrationNumber( 4, "1.10.1" )]
    public class FormattingUpdates : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            BEMA_ReportingTools_ufn_GetFamilyNamesNoTitle();
            BEMA_ReportingTools_ufn_GetFamilyTitles();
            BEMA_ReportingTools_ufn_GetFamilyNickNames();
            BEMA_ReportingTools_ufn_GetFamilyLastNames();

            BEMA_ReportingTools_ufn_GetFamilyTitleFormal();
            BEMA_ReportingTools_sp_ReportingFieldsDataset();
        }

        private void BEMA_ReportingTools_ufn_GetFamilyTitles()
        {
            Sql( @"ALTER FUNCTION [dbo].[BEMA_ReportingTools_ufn_GetFamilyTitles] (
    @PersonId INT
    ,@GroupId INT
    ,@GroupPersonIds VARCHAR(max) = NULL
    ,@UseNickName BIT = 0
	,@FormatValue BIT = 0
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
        JOIN [Person] [p] ON [p].[Id] = [gm].[PersonId] and p.IsDeceased != 1
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

			IF @FormatValue = 0
			BEGIN
				SELECT @GroupTitles = @GroupTitles + case when Title is not null then Title + ' & ' else '' end
				FROM @GroupMemberTable g
				WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
				ORDER BY g.Gender 
					,g.FirstOrNickName
			END

			IF @FormatValue = 1
			BEGIN
				SELECT @GroupTitles = @GroupTitles + case when Title is not null then Title + ' and ' else '' end
				FROM @GroupMemberTable g
				WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
				ORDER BY g.Gender 
					,g.FirstOrNickName
			END
			
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

			  -- cleanup the trailing ' and's
            IF len(@GroupTitles) > 4
            BEGIN
                -- trim the extra ' and' off the end 
                SET @GroupTitles = SUBSTRING(@GroupTitles, 0, len(@GroupTitles) - 3)
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
            Sql( @"ALTER FUNCTION [dbo].[BEMA_ReportingTools_ufn_GetFamilyTitleFormal] (
    @PersonId INT
    ,@GroupId INT
    ,@GroupPersonIds VARCHAR(max) = NULL
    ,@UseNickName BIT = 0
	,@FormatValue BIT = 0
    )
RETURNS @PersonNamesTable TABLE (PersonNames VARCHAR(max))
AS
BEGIN
    DECLARE @PersonNames VARCHAR(max);
    DECLARE @AdultLastNameCount INT;
	DECLARE @GroupMemberCount INT;
	DECLARE @AdultGroupMemberCount INT;
    DECLARE @GroupFirstOrNickNames VARCHAR(max) = '';
    DECLARE @GroupLastName VARCHAR(max);
    DECLARE @GroupAdultFullNames VARCHAR(max) = '';
    DECLARE @GroupNonAdultFullNames VARCHAR(max) = '';
	DECLARE @GroupTitles VARCHAR(max) = '';
	DECLARE @GroupSuffixes VARCHAR(max) = '';
    DECLARE @GroupMemberTable TABLE (
		Title VARCHAR(max)
        ,IsTitleFormal bit
        ,LastName VARCHAR(max)
        ,FirstOrNickName VARCHAR(max)
        ,FullName VARCHAR(max)
		,FullNameWTitle VARCHAR(max)
		,FullNameWTitleComma VARCHAR(max)
		,Suffix VARCHAR(MAX)
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
			,av.ValueAsBoolean as [IsTitleFormal]
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
			 ,CASE @UseNickName
                WHEN 1
                    THEN case when dvtitle.value is not null then [dvtitle].[Value] + ' ' else '' end + ' ' + ISNULL([p].[NickName], '')
                ELSE case when dvtitle.value is not null then [dvtitle].[Value] + ' ' else '' end + ' ' + ISNULL([p].[FirstName], '')
                END + ' ' + ISNULL([p].[LastName], '') + case when dv.Value is null then '' else ', ' +[dv].[Value] end [FullNameWTitleComma]
			,dv.Value as [Suffix]
            ,[p].Gender
            ,[gr].[Guid]
        FROM [GroupMember] [gm]
        JOIN [Person] [p] ON [p].[Id] = [gm].[PersonId] and p.IsDeceased != 1
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
		LEFT OUTER JOIN [DefinedValue] [dvtitle] ON [dvtitle].[Id] = [p].[TitleValueId]
		LEFT OUTER JOIN DefinedType dt on dt.Id = dvtitle.DefinedTypeId
		Left Outer Join Attribute a on a.EntityTypeQualifierColumn = 'DefinedTypeId' and a.EntityTypeQualifierValue = convert(nvarchar(max),dt.Id) and a.[Key] = 'IsFormal'
		Left Outer Join AttributeValue av on a.Id = av.AttributeId and av.EntityId = dvtitle.Id
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
		SET @AdultGroupMemberCount = (Select Count(0) From @GroupMemberTable WHERE [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT)

        -- determine adultCount and if we can use the same lastname for all adults, and get lastname while we are at it
        SELECT @AdultLastNameCount = count(DISTINCT [LastName])
            ,@GroupLastName = max([LastName])
        FROM @GroupMemberTable
        WHERE [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT;
		
		IF @FormatValue = 0
		BEGIN
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

		/********************************** BEGIN *****************************/
        IF @FormatValue = 1
		BEGIN
			IF @AdultLastNameCount > 0
			BEGIN
				Declare @FemaleFormalTitleCount int = (
						Select count(0)
						From @GroupMemberTable
						Where Gender =2
						And IsTitleFormal = 1
						And [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
						)
				Declare @FormalTitleCount int = (
						Select count(0)
						From @GroupMemberTable
						Where IsTitleFormal = 1
						And [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
						)
				Declare @FemaleSuffixCount int = (
						Select count(0)
						From @GroupMemberTable
						Where Gender =2
						And Suffix is not null
						And Suffix != ''
						And [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
						)
				Declare @SuffixCount int = (
						Select count(0)
						From @GroupMemberTable
						Where Suffix is not null
						And Suffix != ''
						And [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
						)
				Declare @FemaleCount int = (
						Select count(0)
						From @GroupMemberTable
						Where Gender =2
						And [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
						)
				Declare @MaleCount int = (
						Select count(0)
						From @GroupMemberTable
						Where Gender != 2
						And [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
						)


				SELECT	 @GroupTitles = @GroupTitles + 
										case when Title is not null then Title +' ' else '' end +' and '
				FROM @GroupMemberTable g
				WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
				ORDER BY g.Gender
					,g.FirstOrNickName
			
				SELECT	 @GroupSuffixes = @GroupSuffixes + 
										case when Suffix is not null then Suffix +' ' else '' end +' and '
				FROM @GroupMemberTable g
				WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
				And Gender != 2
				ORDER BY g.Gender
					,g.FirstOrNickName
			
				IF @GroupTitles is not null
				BEGIN
					-- get the FirstNames and Adult FullNames for use in the cases of families with Adults
					SELECT @GroupFirstOrNickNames = [FirstOrNickName] + ' and '
					FROM @GroupMemberTable g
					WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
				And g.Gender != 2
					ORDER BY g.Gender desc
						,g.FirstOrNickName

					SELECT @GroupAdultFullNames =  @GroupAdultFullNames + IsNull([FullNameWTitleComma], FullName) + ' and '
					FROM @GroupMemberTable g
					WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
					ORDER BY g.Gender desc
						,g.FirstOrNickName
				END
				ELSE
				Begin
				SELECT @GroupFirstOrNickNames = @GroupFirstOrNickNames + [FirstOrNickName] + ' and '
				FROM @GroupMemberTable g
				WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
				And g.Gender != 2
				ORDER BY g.Gender desc
					,g.FirstOrNickName

				SELECT @GroupAdultFullNames = @GroupAdultFullNames + [FullName] + 'and '
				FROM @GroupMemberTable g
				WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
				ORDER BY g.Gender desc
					,g.FirstOrNickName
				End

				-- cleanup the trailing ' &'s
				IF len(@GroupTitles) > 4
				BEGIN
					-- trim the extra ' &' off the end 
					SET @GroupTitles = SUBSTRING(@GroupTitles, 0, len(@GroupTitles) - 3)
				END

				-- cleanup the trailing ' &'s
				IF len(@GroupSuffixes) > 4
				BEGIN
					-- trim the extra ' &' off the end 
					SET @GroupSuffixes = SUBSTRING(@GroupSuffixes, 0, len(@GroupSuffixes) - 3)
				END
				
				-- cleanup the trailing ' &'s
				IF len(@GroupFirstOrNickNames) > 4
				BEGIN
					-- trim the extra ' &' off the end 
					SET @GroupFirstOrNickNames = SUBSTRING(@GroupFirstOrNickNames, 0, len(@GroupFirstOrNickNames) - 3)
				END

				IF len(@GroupAdultFullNames) > 4
				BEGIN
					-- trim the extra ' &' off the end 
					SET @GroupAdultFullNames = SUBSTRING(@GroupAdultFullNames, 0, len(@GroupAdultFullNames) - 3)
				END

				-- if all the firstnames are blanks, get rid of the '&'
				IF (LTRIM(RTRIM(@GroupFirstOrNickNames)) = 'and')
				BEGIN
					SET @GroupFirstOrNickNames = ''
				END
				
				-- if all the titles are blanks, get rid of the '&'
				IF (LTRIM(RTRIM(@GroupTitles)) = 'and')
				BEGIN
					SET @GroupTitles = ''
				END

				-- if all the titles are blanks, get rid of the '&'
				IF (LTRIM(RTRIM(@GroupSuffixes)) = 'and')
				BEGIN
					SET @GroupSuffixes = ''
				END

				-- if all the fullnames are blanks, get rid of the '&'
				IF (LTRIM(RTRIM(@GroupAdultFullNames)) = 'and')
				BEGIN
					SET @GroupAdultFullNames = ''
				END
			END

			IF @GroupMemberCount = 1 or @AdultGroupMemberCount = 1
			Begin
				SET @PersonNames = @GroupAdultFullNames;
			End
			Else
			begin
				IF @AdultLastNameCount = 0
				BEGIN
					-- get the NonAdultFullNames for use in the case of families without adults 
					SELECT @GroupNonAdultFullNames = @groupNonAdultFullNames + [FullName] + ' and '
					FROM @GroupMemberTable
					ORDER BY [FullName]

					IF len(@GroupNonAdultFullNames) > 4
					BEGIN
						-- trim the extra ' &' off the end 
						SET @GroupNonAdultFullNames = SUBSTRING(@GroupNonAdultFullNames, 0, len(@GroupNonAdultFullNames) - 3)
					END

					-- if all the fullnames are blanks, get rid of the '&'
					IF (LTRIM(RTRIM(@GroupNonAdultFullNames)) = 'and')
					BEGIN
						SET @GroupNonAdultFullNames = ''
					END
				END

				IF ( @AdultLastNameCount = 1
					and @FemaleFormalTitleCount = 0
					and @FemaleSuffixCount = 0
					and @FormalTitleCount < 2
					and @SuffixCount < 2
					And @FemaleCount < 2
					And @MaleCount < 2
					)
				BEGIN
					-- just one lastname and at least one adult. Get the Person Names portion of the address in the format <MaleAdult> & <FemaleAdult> <LastName>
					IF (@GroupTitles is not null)
					SET @PersonNames = @GroupTitles + @GroupFirstOrNickNames+ ' ' + @GroupLastName;
					ELSE 
					SET @PersonNames = @GroupFirstOrNickNames + ' ' + @GroupLastName;

					IF (@GroupSuffixes is not null and @GroupSuffixes != '')
					SET @PersonNames = @PersonNames + ', ' + @GroupSuffixes;
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
		/********************************************************* END ******************************/
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
ALTER FUNCTION [dbo].[BEMA_ReportingTools_ufn_GetFamilyNickNames] (
    @PersonId INT
    ,@GroupId INT
    ,@GroupPersonIds VARCHAR(max) = NULL
    ,@UseNickName BIT = 0
	,@FormatValue BIT = 0
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
        JOIN [Person] [p] ON [p].[Id] = [gm].[PersonId] and p.IsDeceased != 1
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
            IF @FormatValue = 0
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
			
			IF @FormatValue = 1
			BEGIN
				-- get the FirstNames and Adult FullNames for use in the cases of families with Adults
				SELECT @GroupFirstOrNickNames = @GroupFirstOrNickNames + [FirstOrNickName] + ' and '
					,@GroupAdultFullNames = @GroupAdultFullNames + [FullName] + ' and '
				FROM @GroupMemberTable g
				WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
				ORDER BY g.Gender desc
					,g.FirstOrNickName

				-- cleanup the trailing ' and's
				IF len(@GroupFirstOrNickNames) > 4
				BEGIN
					-- trim the extra ' and' off the end 
					SET @GroupFirstOrNickNames = SUBSTRING(@GroupFirstOrNickNames, 0, len(@GroupFirstOrNickNames) - 3)
				END

				IF len(@GroupAdultFullNames) > 4
				BEGIN
					-- trim the extra ' and' off the end 
					SET @GroupAdultFullNames = SUBSTRING(@GroupAdultFullNames, 0, len(@GroupAdultFullNames) - 3)
				END

				-- if all the firstnames are blanks, get rid of the 'and'
				IF (LTRIM(RTRIM(@GroupFirstOrNickNames)) = 'and')
				BEGIN
					SET @GroupFirstOrNickNames = ''
				END

				-- if all the fullnames are blanks, get rid of the 'and'
				IF (LTRIM(RTRIM(@GroupAdultFullNames)) = 'and')
				BEGIN
					SET @GroupAdultFullNames = ''
				END
			END
        END

        IF @AdultLastNameCount = 0
        BEGIN
            IF @FormatValue = 0
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
			
			IF @FormatValue = 1
			BEGIN
				-- get the NonAdultFullNames for use in the case of families without adults 
				SELECT @GroupNonAdultFullNames = @groupNonAdultFullNames + [FullName] + ' and '
				FROM @GroupMemberTable
				ORDER BY [FullName]

				IF len(@GroupNonAdultFullNames) > 4
				BEGIN
					-- trim the extra ' and' off the end 
					SET @GroupNonAdultFullNames = SUBSTRING(@GroupNonAdultFullNames, 0, len(@GroupNonAdultFullNames) - 3)
				END

				-- if all the fullnames are blanks, get rid of the 'and'
				IF (LTRIM(RTRIM(@GroupNonAdultFullNames)) = 'and')
				BEGIN
					SET @GroupNonAdultFullNames = ''
				END
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

    IF @FormatValue = 0
	BEGIN
		WHILE (len(@PersonNames) - len(replace(@PersonNames, ' & ', '  ')) > 1)
		BEGIN
			SET @PersonNames = Stuff(@PersonNames, CharIndex(' & ', @PersonNames), Len(' & '), ',')
		END
	END

	IF @FormatValue = 1
	BEGIN
		WHILE (len(@PersonNames) - len(replace(@PersonNames, ' and ', '    ')) > 1)
		BEGIN
			SET @PersonNames = Stuff(@PersonNames, CharIndex(' and ', @PersonNames), Len(' and '), ',')
		END
	END

    INSERT INTO @PersonNamesTable ([PersonNames])
    VALUES (@PersonNames);

    RETURN
END" );
        }

        private void BEMA_ReportingTools_ufn_GetFamilyLastNames()
        {
            Sql( @"ALTER FUNCTION [dbo].[BEMA_ReportingTools_ufn_GetFamilyLastNames] (
    @PersonId INT
    ,@GroupId INT
    ,@GroupPersonIds VARCHAR(max) = NULL
    ,@UseNickName BIT = 0
	,@FormatValue BIT = 0
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
        JOIN [Person] [p] ON [p].[Id] = [gm].[PersonId] and p.IsDeceased != 1
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
            IF @FormatValue = 0
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
			
			IF @FormatValue = 1
			BEGIN
				-- get the FirstNames and Adult FullNames for use in the cases of families with Adults
				SELECT @GroupFirstOrNickNames = @GroupFirstOrNickNames + [FirstOrNickName] + ' and '
					,@GroupAdultFullNames = @GroupAdultFullNames + [FullName] + ' and '
				FROM @GroupMemberTable g
				WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
				ORDER BY g.Gender desc
					,g.FirstOrNickName

				-- cleanup the trailing ' and's
				IF len(@GroupFirstOrNickNames) > 4
				BEGIN
					-- trim the extra ' and' off the end 
					SET @GroupFirstOrNickNames = SUBSTRING(@GroupFirstOrNickNames, 0, len(@GroupFirstOrNickNames) - 3)
				END

				IF len(@GroupAdultFullNames) > 4
				BEGIN
					-- trim the extra ' and' off the end 
					SET @GroupAdultFullNames = SUBSTRING(@GroupAdultFullNames, 0, len(@GroupAdultFullNames) - 3)
				END

				-- if all the firstnames are blanks, get rid of the 'and'
				IF (LTRIM(RTRIM(@GroupFirstOrNickNames)) = 'and')
				BEGIN
					SET @GroupFirstOrNickNames = ''
				END

				-- if all the fullnames are blanks, get rid of the 'and'
				IF (LTRIM(RTRIM(@GroupAdultFullNames)) = 'and')
				BEGIN
					SET @GroupAdultFullNames = ''
				END
			END
        END

        IF @AdultLastNameCount = 0
        BEGIN
            IF @FormatValue = 0
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
			
			IF @FormatValue = 1
			BEGIN
				-- get the NonAdultFullNames for use in the case of families without adults 
				SELECT @GroupNonAdultFullNames = @groupNonAdultFullNames + [FullName] + ' and '
				FROM @GroupMemberTable
				ORDER BY [FullName]

				IF len(@GroupNonAdultFullNames) > 4
				BEGIN
					-- trim the extra ' and' off the end 
					SET @GroupNonAdultFullNames = SUBSTRING(@GroupNonAdultFullNames, 0, len(@GroupNonAdultFullNames) - 3)
				END

				-- if all the fullnames are blanks, get rid of the 'and'
				IF (LTRIM(RTRIM(@GroupNonAdultFullNames)) = 'and')
				BEGIN
					SET @GroupNonAdultFullNames = ''
				END
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

    IF @FormatValue = 0
	BEGIN
		WHILE (len(@PersonNames) - len(replace(@PersonNames, ' & ', '  ')) > 1)
		BEGIN
			SET @PersonNames = Stuff(@PersonNames, CharIndex(' & ', @PersonNames), Len(' & '), ',')
		END
	END

	IF @FormatValue = 1
	BEGIN
		WHILE (len(@PersonNames) - len(replace(@PersonNames, ' and ', '    ')) > 1)
		BEGIN
			SET @PersonNames = Stuff(@PersonNames, CharIndex(' and ', @PersonNames), Len(' and '), ',')
		END
	END

	SET @PersonNames = Replace(@PersonNames, '  ',' ')

    INSERT INTO @PersonNamesTable ([PersonNames])
    VALUES (@PersonNames);

    RETURN
END" );
        }

        private void BEMA_ReportingTools_ufn_GetFamilyNamesNoTitle()
        {
            Sql( @"CREATE FUNCTION [dbo].[BEMA_ReportingTools_ufn_GetFamilyNamesNoTitle] (
    @PersonId INT
    ,@GroupId INT
    ,@GroupPersonIds VARCHAR(max) = NULL
    ,@UseNickName BIT = 0
	,@IncludeInactive BIT = 1
	,@FirstNameNickNameNoTitleFormatValue BIT = 0
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
	DECLARE @cPERSON_RECORD_STATUS_ACTIVE_ID INT = (SELECT TOP 1 ID FROM DefinedValue where [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E');


    IF (@PersonId IS NOT NULL)
    BEGIN
        -- just getting the Person Names portion of the address for an individual person
        SELECT @PersonNames = CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END + ' ' + ISNULL([p].[LastName], '') + ISNULL(' ' + [dv].[Value], '')
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
                END + ' ' + ISNULL([p].[LastName], '') + ISNULL(' ' + [dv].[Value], '') [FullName]
            ,[p].Gender
            ,[gr].[Guid]
        FROM [GroupMember] [gm]
        JOIN [Person] [p] ON [p].[Id] = [gm].[PersonId] and p.IsDeceased != 1
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
        JOIN [GroupTypeRole] [gr] ON [gm].[GroupRoleId] = [gr].[Id]
        WHERE [GroupId] = @GroupId
			AND (@IncludeInactive = 1 OR P.RecordStatusValueId = @cPERSON_RECORD_STATUS_ACTIVE_ID)
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
			IF @FirstNameNickNameNoTitleFormatValue = 0
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
			
			IF @FirstNameNickNameNoTitleFormatValue = 1
			BEGIN
				-- get the FirstNames and Adult FullNames for use in the cases of families with Adults
				SELECT @GroupFirstOrNickNames = @GroupFirstOrNickNames + [FirstOrNickName] + ' and '
					,@GroupAdultFullNames = @GroupAdultFullNames + [FullName] + ' and '
				FROM @GroupMemberTable g
				WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
				ORDER BY g.Gender desc
					,g.FirstOrNickName

				-- cleanup the trailing ' and's
				IF len(@GroupFirstOrNickNames) > 4
				BEGIN
					-- trim the extra ' and' off the end 
					SET @GroupFirstOrNickNames = SUBSTRING(@GroupFirstOrNickNames, 0, len(@GroupFirstOrNickNames) - 3)
				END

				IF len(@GroupAdultFullNames) > 4
				BEGIN
					-- trim the extra ' and' off the end 
					SET @GroupAdultFullNames = SUBSTRING(@GroupAdultFullNames, 0, len(@GroupAdultFullNames) - 3)
				END

				-- if all the firstnames are blanks, get rid of the 'and'
				IF (LTRIM(RTRIM(@GroupFirstOrNickNames)) = 'and')
				BEGIN
					SET @GroupFirstOrNickNames = ''
				END

				-- if all the fullnames are blanks, get rid of the 'and'
				IF (LTRIM(RTRIM(@GroupAdultFullNames)) = 'and')
				BEGIN
					SET @GroupAdultFullNames = ''
				END
			END
        END

        IF @AdultLastNameCount = 0
        BEGIN
			IF @FirstNameNickNameNoTitleFormatValue = 0
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
			
			IF @FirstNameNickNameNoTitleFormatValue = 1
			BEGIN
				-- get the NonAdultFullNames for use in the case of families without adults 
				SELECT @GroupNonAdultFullNames = @groupNonAdultFullNames + [FullName] + ' and '
				FROM @GroupMemberTable
				ORDER BY [FullName]

				IF len(@GroupNonAdultFullNames) > 4
				BEGIN
					-- trim the extra ' and' off the end 
					SET @GroupNonAdultFullNames = SUBSTRING(@GroupNonAdultFullNames, 0, len(@GroupNonAdultFullNames) - 3)
				END

				-- if all the fullnames are blanks, get rid of the 'and'
				IF (LTRIM(RTRIM(@GroupNonAdultFullNames)) = 'and')
				BEGIN
					SET @GroupNonAdultFullNames = ''
				END
			END
        END

        IF (@AdultLastNameCount = 1)
        BEGIN
            -- just one lastname and at least one adult. Get the Person Names portion of the address in the format <MaleAdult> & <FemaleAdult> <LastName>
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

	IF @FirstNameNickNameNoTitleFormatValue = 0
	BEGIN
		WHILE (len(@PersonNames) - len(replace(@PersonNames, ' & ', '  ')) > 1)
		BEGIN
			SET @PersonNames = Stuff(@PersonNames, CharIndex(' & ', @PersonNames), Len(' & '), ',')
		END
	END

	IF @FirstNameNickNameNoTitleFormatValue = 1
	BEGIN
		WHILE (len(@PersonNames) - len(replace(@PersonNames, ' and ', '    ')) > 1)
		BEGIN
			SET @PersonNames = Stuff(@PersonNames, CharIndex(' and ', @PersonNames), Len(' and '), ',')
		END
	END

    INSERT INTO @PersonNamesTable ([PersonNames])
    VALUES (@PersonNames);

    RETURN
END" );
        }

        private void BEMA_ReportingTools_sp_ReportingFieldsDataset()
        {
            Sql( @"
ALTER PROCEDURE [dbo].[BEMA_ReportingTools_sp_ReportingFieldsDataset]
	 @firstNameNickNameNoTitleFormatValue INT = 0,
	 @firstNameFormatValue INT = 0,
	 @fullNameNickNameFormatValue INT = 0,
	 @lastNameFormatValue INT = 0,
	 @nickNameFormatValue INT = 0,
	 @titleFormatValue INT = 0,
	 @fullNameFirstNameFormatValue INT = 0
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
	Select top 10 p.Id, 
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
	Join Person p on p.Id = familyMembers.familyMemberId and p.IsDeceased != 1
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
			
			(SELECT * FROM dbo.BEMA_ReportingTools_ufn_GetFamilyTitleFormal(null, mpId.GroupId, default, 1, @fullNameNickNameFormatValue)) 
				as FamilyFullNameNickName,
			(SELECT * FROM dbo.BEMA_ReportingTools_ufn_GetFamilyTitleFormal( null, mpId.GroupId, mpId.GivingGroupPersonIds, 1,@fullNameNickNameFormatValue) )
				as GivingUnitFullNameNickName,				
			(SELECT * FROM dbo.BEMA_ReportingTools_ufn_GetFamilyTitleFormal(null, mpId.GroupId, default, 0, @fullNameFirstNameFormatValue)) 
				as FamilyFullNameFirstName,
			(SELECT * FROM dbo.BEMA_ReportingTools_ufn_GetFamilyTitleFormal(null, mpId.GroupId, mpId.GivingGroupPersonIds, 0, @fullNameFirstNameFormatValue)) 
				as GivingUnitFullNameFirstName,

			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyNamesNoTitle]( null, mpId.GroupId, default, 1,default, @firstNameNickNameNoTitleFormatValue)) 
				as FamilyFullNameNickNameNoTitle,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyNamesNoTitle]( null, mpId.GroupId,mpId.GivingGroupPersonIds , 1,default, @firstNameNickNameNoTitleFormatValue) )
				as GivingUnitFullNameNickNameNoTitle,

			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyNickNames](null, mpId.GroupId, default, 0, @firstNameFormatValue))
				as FamilyFirstName,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyNickNames]( null, mpId.GroupId, mpId.GivingGroupPersonIds, 0, @firstNameFormatValue) )
				as GivingUnitFirstName,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyNickNames](null, mpId.GroupId, default, 1, @nickNameFormatValue))
				as FamilyNickNames,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyNickNames](null, mpId.GroupId, mpId.GivingGroupPersonIds, 1, @nickNameFormatValue))
				as GivingUnitNickNames,

			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyLastNames](null, mpId.GroupId, default, 1, @lastNameFormatValue))
				as FamilyLastNames,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyLastNames](null, mpId.GroupId, mpId.GivingGroupPersonIds, 1, @lastNameFormatValue))
				as GivingUnitLastNames,

			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyTitles](null, mpId.GroupId, default, 1, @titleFormatValue)) 
				as FamilyTitles,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyTitles](null, mpId.GroupId, mpId.GivingGroupPersonIds, 1, @titleFormatValue)) 
				as GivingUnitTitles
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

