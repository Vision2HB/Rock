
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
			GroupId int null
			)

	Insert into @ModifiedPersonIds
	Select p.Id, familyMembers.GroupId
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


	Select	p.Id,
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
			(SELECT * FROM dbo.ufnCrm_GetFamilyTitle( null, mpId.GroupId, (Select String_agg(p1.Id,',') From Person p1 where p1.GivingId = p.GivingId), 1) )
				as GivingUnitFullNameNickNameNoTitle,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyNickNames](null, mpId.GroupId, default, 0))
				as FamilyFirstName,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyNickNames]( null, mpId.GroupId, (Select String_agg(p1.Id,',') From Person p1 where p1.GivingId = p.GivingId), 0) )
				as GivingUnitFirstName,
			(SELECT * FROM dbo.BEMA_ReportingTools_ufn_GetFamilyTitleFormal(null, mpId.GroupId, default, 1)) 
				as FamilyFullNameNickName,
			(SELECT * FROM dbo.BEMA_ReportingTools_ufn_GetFamilyTitleFormal( null, mpId.GroupId, (Select String_agg(p1.Id,',') From Person p1 where p1.GivingId = p.GivingId), 1) )
				as GivingUnitFullNameNickName,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyLastNames](null, mpId.GroupId, default, 1))
				as FamilyLastNames,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyLastNames](null, mpId.GroupId, (Select String_agg(p1.Id,',') From Person p1 where p1.GivingId = p.GivingId), 1))
				as GivingUnitLastNames,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyNickNames](null, mpId.GroupId, default, 1))
				as FamilyNickNames,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyNickNames](null, mpId.GroupId, (Select String_agg(p1.Id,',') From Person p1 where p1.GivingId = p.GivingId), 1))
				as GivingUnitNickNames,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyTitles](null, mpId.GroupId, default, 1)) 
				as FamilyTitles,
			(SELECT * FROM dbo.[BEMA_ReportingTools_ufn_GetFamilyTitles](null, mpId.GroupId, (Select String_agg(p1.Id,',') From Person p1 where p1.GivingId = p.GivingId), 1)) 
				as GivingUnitTitles,
			(SELECT * FROM dbo.BEMA_ReportingTools_ufn_GetFamilyTitleFormal(null, mpId.GroupId, default, 0)) 
				as FamilyFullNameFirstName,
			(SELECT * FROM dbo.BEMA_ReportingTools_ufn_GetFamilyTitleFormal(null, mpId.GroupId, (Select String_agg(p1.Id,',') From Person p1 where p1.GivingId = p.GivingId), 0)) 
				as GivingUnitFullNameFirstName
	From Person p
	Join (Select distinct * From @ModifiedPersonIds) mpId on p.Id = mpId.PersonId
	Order By LastName desc, FirstName 


END
GO


