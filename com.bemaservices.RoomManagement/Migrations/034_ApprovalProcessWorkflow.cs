﻿// <copyright>
// Copyright by BEMA Software Services
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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 34, "1.10.3" )]
    public class ApprovalProcessWorkflow : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateFieldType( "Reservation Type", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.ReservationTypeFieldType", "1AF29883-3028-4867-8DC7-0848953E8B6C" );
            RockMigrationHelper.UpdateFieldType( "Resource", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.ResourceFieldType", "7CFF9796-C8A1-4544-A90C-9CA0C07C27D6" );

            AddReservationCreationWorkflowActions();

            SpecialApprovalNotification();
            ApprovalProcess();

            RockMigrationHelper.UpdateWorkflowType( false, false, "(Depreciated) Room Reservation Approval Notification", "A workflow that sends an email to the party responsible for the next step in the room reservation approval process.", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Approval Request", "fa fa-list-ol", 28800, true, 0, "543D4FCD-310B-4048-BFCB-BAE582CBB890", 0 ); // (Depreciated) Room Reservation Approval Notification
            RockMigrationHelper.UpdateWorkflowType( false, true, "Reminder Notification", "Used for sending a reminder email to the event contact regarding their upcoming resource reservation. IMPORTANT NOTE: This workflow contains Lava that uses the 'execute' Lava command. (Execute needs to be enabled in Global Attributes: 'Default Enabled Lava Commands')", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Reservation Reminders", "fa fa-list-ol", 28800, true, 0, "A219357D-4992-415E-BF5F-33C242BB3BD2", 0 ); // Reminder Notification
            RockMigrationHelper.UpdateWorkflowType( false, true, "Post-Approval Modification Process", "A workflow that changes the reservation's approval status if it was modified by someone not in the Final Approval Group after being approved.", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Approval Update", "fa fa-list-ol", 28800, false, 0, "13D0361C-0552-43CA-8F27-D47DB120608D", 0 ); // Post-Approval Modification Process

            Sql( @"
                    Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] Where [Guid] in ('5339e1c4-ac09-4bd5-9416-628dba200ba5','68f6de62-cdbb-4ec0-8440-8b1740c21e65')
                    DECLARE @WorkflowId int = (Select Top 1 Id From WorkflowType Where Guid = '83907883-4803-4AFB-8A20-49FDC0BE4788')
                    INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] ([WorkflowTypeId],[ReservationTypeId], [TriggerType], [QualifierValue], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [Guid], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES (@WorkflowId,1, 2, N'|||', CAST(N'2020-10-23 14:02:11.953' AS DateTime), CAST(N'2020-10-23 14:02:11.953' AS DateTime), NULL, NULL, N'68F6DE62-CDBB-4EC0-8440-8B1740C21E65', NULL, NULL, NULL)
            " );
        }

        private void AddReservationCreationWorkflowActions()
        {
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationLocation", "514493E9-4688-4926-9BCB-B945C8722578", false, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationResource", "2441F4FC-3812-4511-9E55-6BA46141D767", false, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation", "36B233BE-A202-4D58-B1AE-00A49EC20D44", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2441F4FC-3812-4511-9E55-6BA46141D767", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "C8AC59C8-681A-4050-A9EC-4CB7B700E4AB" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationResource:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2441F4FC-3812-4511-9E55-6BA46141D767", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Attribute", "ReservationAttribute", "The attribute that contains the reservation.", 0, @"", "5AD0666A-C616-48A8-A97A-EAE9B68EAA4E" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationResource:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2441F4FC-3812-4511-9E55-6BA46141D767", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Resource Attribute", "ResourceAttribute", "The attribute that contains the resource.", 1, @"", "2166489E-0975-40B8-AC1B-02BB1F0495BD" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationResource:Resource Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2441F4FC-3812-4511-9E55-6BA46141D767", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Quantity|Attribute Value", "Quantity", "The quantity or an attribute that contains the quantity of the resource. <span class='tip tip-lava'></span>", 0, @"", "1FB9F74A-C771-49CE-86A2-96D3A5065ACD" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationResource:Quantity|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2441F4FC-3812-4511-9E55-6BA46141D767", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "70F5C258-B07B-4866-9300-5407815D2A68" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationResource:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "1AF29883-3028-4867-8DC7-0848953E8B6C", "Reservation Type", "ReservationType", "The reservation type to use (if Reservation Type Attribute is not specified).", 2, @"", "97C5C582-FA66-4E8C-B93A-6BA4562378E4" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Reservation Type
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "EB74366A-FF16-4303-9913-137AC222B2AD" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Approval State Attribute", "ApprovalStateAttribute", "The attribute that contains the reservation approval state.", 3, @"", "B7AECA5C-43E9-4FD6-A281-F15A89E10AFD" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Approval State Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Attribute", "ReservationAttribute", "The reservation attribute to set the value to the reservation created.", 6, @"", "156DEE15-53E0-4D29-8788-1984CC371664" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Type Attribute", "ReservationTypeAttribute", "The attribute that contains the reservation type of the reservation.", 1, @"", "5F754B62-49C5-4CD5-80F3-A58A0E5F5A31" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Reservation Type Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Schedule Attribute", "ScheduleAttribute", "The attribute that contains the reservation schedule.", 5, @"", "3332A18E-D8ED-4233-B5A4-02F79F04C7AD" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Schedule Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Name|Attribute Value", "Name", "The name or an attribute that contains the name of the reservation. <span class='tip tip-lava'></span>", 0, @"", "A2B703AC-9CDE-4501-BDA8-F4D01A93AB2B" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Name|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "63A7ECDC-5AE4-4CEF-AB9F-BE0A27DC7687" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "F4ACC5B8-98BB-4611-B6B7-065BBC47503B", "Approval State", "ApprovalState", "The approval state to use (if Approval State Attribute is not specified).", 4, @"", "512BFEBD-64FF-40B7-BF23-2039285B9E2B" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Approval State
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "514493E9-4688-4926-9BCB-B945C8722578", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "7BB104D9-35AC-431B-9359-E4721BF3CF03" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationLocation:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "514493E9-4688-4926-9BCB-B945C8722578", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Location Attribute", "LocationAttribute", "The attribute that contains the location.", 1, @"", "62CC3DA8-C6F8-4535-9102-B4C32C9191C7" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationLocation:Location Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "514493E9-4688-4926-9BCB-B945C8722578", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Attribute", "ReservationAttribute", "The attribute that contains the reservation.", 0, @"", "6F416249-8B9A-4002-9A2D-4A500235BEC9" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationLocation:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "514493E9-4688-4926-9BCB-B945C8722578", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "5EB174ED-4401-4E46-AABD-4D56C94FEC13" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationLocation:Order
        }

        private void SpecialApprovalNotification()
        {
            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendEmail", "66197B01-D1F0-4924-A315-47AD54E030DE", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body", "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 4, @"", "4D245B9E-6B03-46E7-8482-A51FBA190E4D" ); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36197160-7D3D-490D-AB42-7E29105AFE91" ); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "Should a record of this communication be saved to the recipient's profile", 8, @"False", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3" ); // Rock.Workflow.Action.SendEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment One", "AttachmentOne", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 5, @"", "C2C7DA55-3018-4645-B9EE-4BCD11855F2C" ); // Rock.Workflow.Action.SendEmail:Attachment One
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Three", "AttachmentThree", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 7, @"", "A059767A-5592-4926-948A-1065AF4E9748" ); // Rock.Workflow.Action.SendEmail:Attachment Three
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Two", "AttachmentTwo", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 6, @"", "FFD9193A-451F-40E6-9776-74D5DCAC1450" ); // Rock.Workflow.Action.SendEmail:Attachment Two
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Send to Group Role", "GroupRole", "An optional Group Role attribute to limit recipients to if the 'Send to Email Address' is a group or security role.", 2, @"", "E3667110-339F-4FE3-B6B7-084CF9633580" ); // Rock.Workflow.Action.SendEmail:Send to Group Role
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "From Email Address|Attribute Value", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" ); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Addresses|Attribute Value", "To", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", 1, @"", "0C4C13B8-7076-4872-925A-F950886B5E16" ); // Rock.Workflow.Action.SendEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", 3, @"", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386" ); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1269254-C15A-40BD-B784-ADCC231D3950" ); // Rock.Workflow.Action.SendEmail:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Room Management", "fa fa-building-o", "", "B8E4B3B0-B543-48B6-93BE-604D4F368559", 0 ); // Room Management

            #endregion

            #region Special Approval Notification

            RockMigrationHelper.UpdateWorkflowType( false, true, "Special Approval Notification", "", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Work", "fa fa-list-ol", 28800, true, 0, "66899922-D665-4839-8742-BD8556D7FB61", 0 ); // Special Approval Notification
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "66899922-D665-4839-8742-BD8556D7FB61", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Approval Group", "ApprovalGroup", "", 0, @"", "8F0172A6-3C38-4B62-AD4B-76AEBF19905F", false ); // Special Approval Notification:Approval Group
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "66899922-D665-4839-8742-BD8556D7FB61", "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7", "Reservation", "Reservation", "", 1, @"", "C39F0BCB-A832-4D99-ACC1-568C8C6BA202", false ); // Special Approval Notification:Reservation
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "66899922-D665-4839-8742-BD8556D7FB61", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Relevant Item", "RelevantItem", "", 2, @"", "B8F2580D-64F1-4610-9E93-6F0DE2CEFAFB", false ); // Special Approval Notification:Relevant Item
            RockMigrationHelper.AddAttributeQualifier( "B8F2580D-64F1-4610-9E93-6F0DE2CEFAFB", "ispassword", @"False", "C7183398-3221-4AE3-9312-2D4C930D724E" ); // Special Approval Notification:Relevant Item:ispassword
            RockMigrationHelper.AddAttributeQualifier( "B8F2580D-64F1-4610-9E93-6F0DE2CEFAFB", "maxcharacters", @"", "82AE881A-AEE2-4172-B27C-FE12E7950B01" ); // Special Approval Notification:Relevant Item:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "B8F2580D-64F1-4610-9E93-6F0DE2CEFAFB", "showcountdown", @"False", "78A68997-C5D2-41A1-AA53-9F6BA8F82E3C" ); // Special Approval Notification:Relevant Item:showcountdown
            RockMigrationHelper.UpdateWorkflowActivityType( "66899922-D665-4839-8742-BD8556D7FB61", true, "Start", "", true, 0, "E92F7E39-7C7B-45E4-97BA-7ECD197E7642" ); // Special Approval Notification:Start
            RockMigrationHelper.UpdateWorkflowActionType( "E92F7E39-7C7B-45E4-97BA-7ECD197E7642", "Send Notification Email", 0, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "F07AA85B-B666-4D8C-B770-7B5377A9EF34" ); // Special Approval Notification:Start:Send Notification Email
            RockMigrationHelper.AddActionTypeAttributeValue( "F07AA85B-B666-4D8C-B770-7B5377A9EF34", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Special Approval Notification:Start:Send Notification Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F07AA85B-B666-4D8C-B770-7B5377A9EF34", "0C4C13B8-7076-4872-925A-F950886B5E16", @"8f0172a6-3c38-4b62-ad4b-76aebf19905f" ); // Special Approval Notification:Start:Send Notification Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "F07AA85B-B666-4D8C-B770-7B5377A9EF34", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Special Approval Needed: {{ Workflow | Attribute:'RelevantItem'}} for {{Workflow | Attribute:'Reservation'}}" ); // Special Approval Notification:Start:Send Notification Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "F07AA85B-B666-4D8C-B770-7B5377A9EF34", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
A new reservation requires your special approval:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Special Approval Notification:Start:Send Notification Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "F07AA85B-B666-4D8C-B770-7B5377A9EF34", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Special Approval Notification:Start:Send Notification Email:Save Communication History

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"
			UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )
			FROM [AttributeQualifier] [aq]
			INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]
			INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
			INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]
			WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'
			AND [aq].[key] = 'definedtypeguid'
		" );

            #endregion        }
        }

        private void ApprovalProcess()
        {
            #region FieldTypes

            RockMigrationHelper.UpdateFieldType( "Reservation Location Approval State", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.ReservationLocationApprovalStateFieldType", "CAA46A41-583F-420F-AB2D-10B7D4B57828" );
            RockMigrationHelper.UpdateFieldType( "Reservation Resource Approval State", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.ReservationResourceApprovalStateFieldType", "F42935CE-9676-4C72-8664-C291C2965C5B" );

            #endregion

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState", "3894452A-E763-41AC-8260-10373646D8A0", false, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationLocationsApprovalStates", "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", false, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationResourcesApprovalStates", "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.ActivateActivity", "38907A90-1634-4A93-8017-619326A4A582", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CompleteWorkflow", "EEDA4318-F014-4A46-9C76-4C052EF81AA1", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.Delay", "D22E73F7-86E2-46CA-AD5B-7770A866726B", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunLava", "BC21E57A-1477-44B3-A7C2-61A806118945", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendEmail", "66197B01-D1F0-4924-A315-47AD54E030DE", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeFromEntity", "972F19B9-598B-474B-97A4-50E56E7B59D2", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeValue", "C789E457-0783-44B3-9D8F-2EBAB5F11110", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetEntityProperty", "C545211C-1143-498E-8B3A-FEE9D59C7C96", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetWorkflowName", "36005473-BD5D-470B-B28D-98E6D7ED808D", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0A800013-51F7-4902-885A-5BE215D67D3D" ); // Rock.Workflow.Action.SetWorkflowName:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "NameValue", "The value to use for the workflow's name. <span class='tip tip-lava'></span>", 1, @"", "93852244-A667-4749-961A-D47F88675BE4" ); // Rock.Workflow.Action.SetWorkflowName:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "5D95C15A-CCAE-40AD-A9DD-F929DA587115" ); // Rock.Workflow.Action.SetWorkflowName:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "E8ABD802-372C-47BE-82B1-96F50DB5169E" ); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "739FD425-5B8C-4605-B775-7E4D9D4C11DB", "Activity", "Activity", "The activity type to activate", 0, @"", "02D5A7A5-8781-46B4-B9FC-AF816829D240" ); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3809A78C-B773-440C-8E3F-A8E81D0DAE08" ); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "ACA008E2-2406-457E-8E4C-6922E03757A4" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Approval State Attribute", "ApprovalStateAttribute", "The attribute that contains the reservation approval state.", 1, @"", "2E185FB5-FC8E-41BE-B7FE-702F74B47539" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Approval State Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Attribute", "ReservationAttribute", "The attribute that contains the reservation.", 0, @"", "1D4F819F-145D-4A7F-AB4E-AD7C06759042" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25954FDC-F486-417D-ABBB-E2DF2C67B186" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "F4ACC5B8-98BB-4611-B6B7-065BBC47503B", "Approval State", "ApprovalState", "The approval state to use (if Approval State Attribute is not specified).", 2, @"", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Approval State
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "15A2EEA8-53DA-4ADD-9DDE-FF2FCA70023F" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationLocationsApprovalStates:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Ignore Locations With Approval Groups", "IgnoreLocationsWithApprovalGroups", "Whether to skip updating the statuses of locations with approval groups", 3, @"True", "22FAA8A0-DE01-4402-B0A6-89A5C58B180A" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationLocationsApprovalStates:Ignore Locations With Approval Groups
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Approval State Attribute", "ApprovalStateAttribute", "The attribute that contains the reservation locations' approval state.", 1, @"", "26E349B1-61EF-441F-8700-A19E9FA7BCED" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationLocationsApprovalStates:Approval State Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Attribute", "ReservationAttribute", "The attribute that contains the reservation.", 0, @"", "D530F451-D1FA-416A-86F1-7E87FBCC4EC7" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationLocationsApprovalStates:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "272F7FD1-ABC9-4B58-94D8-7973A0146330" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationLocationsApprovalStates:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", "CAA46A41-583F-420F-AB2D-10B7D4B57828", "Approval State", "ApprovalState", "The approval state to use (if Approval State Attribute is not specified).", 2, @"", "53B06E3D-22CD-488B-86D3-4C4D81511334" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationLocationsApprovalStates:Approval State
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body", "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 4, @"", "4D245B9E-6B03-46E7-8482-A51FBA190E4D" ); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36197160-7D3D-490D-AB42-7E29105AFE91" ); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "Should a record of this communication be saved to the recipient's profile", 8, @"False", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3" ); // Rock.Workflow.Action.SendEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment One", "AttachmentOne", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 5, @"", "C2C7DA55-3018-4645-B9EE-4BCD11855F2C" ); // Rock.Workflow.Action.SendEmail:Attachment One
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Three", "AttachmentThree", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 7, @"", "A059767A-5592-4926-948A-1065AF4E9748" ); // Rock.Workflow.Action.SendEmail:Attachment Three
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Two", "AttachmentTwo", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 6, @"", "FFD9193A-451F-40E6-9776-74D5DCAC1450" ); // Rock.Workflow.Action.SendEmail:Attachment Two
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Send to Group Role", "GroupRole", "An optional Group Role attribute to limit recipients to if the 'Send to Email Address' is a group or security role.", 2, @"", "E3667110-339F-4FE3-B6B7-084CF9633580" ); // Rock.Workflow.Action.SendEmail:Send to Group Role
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "From Email Address|Attribute Value", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" ); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Addresses|Attribute Value", "To", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", 1, @"", "0C4C13B8-7076-4872-925A-F950886B5E16" ); // Rock.Workflow.Action.SendEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", 3, @"", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386" ); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1269254-C15A-40BD-B784-ADCC231D3950" ); // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>", 4, @"", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199" ); // Rock.Workflow.Action.SetAttributeFromEntity:Lava Template
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1" ); // Rock.Workflow.Action.SetAttributeFromEntity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Entity Is Required", "EntityIsRequired", "Should an error be returned if the entity is missing or not a valid entity type?", 2, @"True", "B524B00C-29CB-49E9-9896-8BB60F209783" ); // Rock.Workflow.Action.SetAttributeFromEntity:Entity Is Required
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Id instead of Guid", "UseId", "Most entity attribute field types expect the Guid of the entity (which is used by default). Select this option if the entity's Id should be used instead (should be rare).", 3, @"False", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B" ); // Rock.Workflow.Action.SetAttributeFromEntity:Use Id instead of Guid
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 1, @"", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7" ); // Rock.Workflow.Action.SetAttributeFromEntity:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB" ); // Rock.Workflow.Action.SetAttributeFromEntity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "C04B4BA1-65E6-493F-BD1B-834D6A1961E8" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationResourcesApprovalStates:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Ignore Resources With Approval Groups", "IgnoreResourcesWithApprovalGroups", "Whether to skip updating the statuses of resources with approval groups", 3, @"True", "3CA5B94F-5571-497C-85EF-2586B62BDFC2" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationResourcesApprovalStates:Ignore Resources With Approval Groups
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Approval State Attribute", "ApprovalStateAttribute", "The attribute that contains the reservation resources' approval state.", 1, @"", "1F7A0B12-5EA3-41AF-913D-F45659E28D59" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationResourcesApprovalStates:Approval State Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Attribute", "ReservationAttribute", "The attribute that contains the reservation.", 0, @"", "4FBD0A05-603C-4F7E-B122-2865F31A4AD0" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationResourcesApprovalStates:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3BE60722-BE2B-4DE5-8CC7-FD3F90EC2373" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationResourcesApprovalStates:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", "F42935CE-9676-4C72-8664-C291C2965C5B", "Approval State", "ApprovalState", "The approval state to use (if Approval State Attribute is not specified).", 2, @"", "0E07B013-2C74-4CD7-AFBC-C535151AA60A" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationResourcesApprovalStates:Approval State
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava", "Value", "The <span class='tip tip-lava'></span> to run.", 0, @"", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4" ); // Rock.Workflow.Action.RunLava:Lava
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "F1924BDC-9B79-4018-9D4A-C3516C87A514" ); // Rock.Workflow.Action.RunLava:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to store the result in.", 1, @"", "431273C6-342D-4030-ADC7-7CDEDC7F8B27" ); // Rock.Workflow.Action.RunLava:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "The Lava commands that should be enabled for this action.", 2, @"", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5" ); // Rock.Workflow.Action.RunLava:Enabled Lava Commands
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "1B833F48-EFC2-4537-B1E3-7793F6863EAA" ); // Rock.Workflow.Action.RunLava:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C545211C-1143-498E-8B3A-FEE9D59C7C96", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "FCA61786-8EC0-44D7-8A3D-152721FF2353" ); // Rock.Workflow.Action.SetEntityProperty:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C545211C-1143-498E-8B3A-FEE9D59C7C96", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "The type of Entity.", 0, @"", "8373A55C-E023-4DE0-B583-06FF906520FC" ); // Rock.Workflow.Action.SetEntityProperty:Entity Type
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C545211C-1143-498E-8B3A-FEE9D59C7C96", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Entity Id or Guid|Entity Attribute", "EntityIdGuid", "The id or guid of the entity. <span class='tip tip-lava'></span>", 1, @"", "913D7A95-BC44-4874-92F9-66DB85DF9FEF" ); // Rock.Workflow.Action.SetEntityProperty:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C545211C-1143-498E-8B3A-FEE9D59C7C96", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Property Name|Property Name Attribute", "PropertyName", "The name of the property to set. <span class='tip tip-lava'></span>", 2, @"", "EF2CEB92-D90D-4533-9A1A-3F61E0E436A5" ); // Rock.Workflow.Action.SetEntityProperty:Property Name|Property Name Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C545211C-1143-498E-8B3A-FEE9D59C7C96", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Property Value|Property Value Attribute", "PropertyValue", "The value to set. <span class='tip tip-lava'></span>", 3, @"", "0415C959-BF89-4D19-9C47-3AB1098E1FBA" ); // Rock.Workflow.Action.SetEntityProperty:Property Value|Property Value Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C545211C-1143-498E-8B3A-FEE9D59C7C96", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Empty Value Handling", "EmptyValueHandling", "How to handle empty property values.", 4, @"", "6C6D7AF8-4A4C-46E7-9F57-F150186B7D2C" ); // Rock.Workflow.Action.SetEntityProperty:Empty Value Handling
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C545211C-1143-498E-8B3A-FEE9D59C7C96", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "22EF2F99-C277-4D3A-A779-E5D4D71D28C5" ); // Rock.Workflow.Action.SetEntityProperty:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "D7EAA859-F500-4521-9523-488B12EAA7D2" ); // Rock.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 0, @"", "44A0B977-4730-4519-8FF6-B0A01A95B212" ); // Rock.Workflow.Action.SetAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "Value", "The text or attribute to set the value from. <span class='tip tip-lava'></span>", 1, @"", "E5272B11-A2B8-49DC-860D-8D574E2BC15C" ); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "57093B41-50ED-48E5-B72B-8829E62704C8" ); // Rock.Workflow.Action.SetAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "D22E73F7-86E2-46CA-AD5B-7770A866726B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "D916CB65-9413-479F-8F5E-6E599CE48025" ); // Rock.Workflow.Action.Delay:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "D22E73F7-86E2-46CA-AD5B-7770A866726B", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Date In Attribute", "DateInAttribute", "The date or date/time attribute value to use for the delay.", 1, @"", "55F1DD31-6F42-464F-A9A1-2B9484C07AB4" ); // Rock.Workflow.Action.Delay:Date In Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "D22E73F7-86E2-46CA-AD5B-7770A866726B", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Next Weekday", "NextWeekday", "The next day of the week to wait till.", 2, @"", "8AE43CB6-2DF9-4DE0-856A-98F846357274" ); // Rock.Workflow.Action.Delay:Next Weekday
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "D22E73F7-86E2-46CA-AD5B-7770A866726B", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minutes To Delay", "MinutesToDelay", "The number of minutes to delay successful execution of action", 0, @"", "3C501BE2-FE9E-479D-BC59-8F3B72FF6E4A" ); // Rock.Workflow.Action.Delay:Minutes To Delay
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "D22E73F7-86E2-46CA-AD5B-7770A866726B", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "77AA4791-AD54-4944-9827-2997BA3B1ED9" ); // Rock.Workflow.Action.Delay:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C" ); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Status|Status Attribute", "Status", "The status to set the workflow to when marking the workflow complete. <span class='tip tip-lava'></span>", 0, @"Completed", "385A255B-9F48-4625-862B-26231DBAC53A" ); // Rock.Workflow.Action.CompleteWorkflow:Status|Status Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25CAD4BE-5A00-409D-9BAB-E32518D89956" ); // Rock.Workflow.Action.CompleteWorkflow:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Room Management", "fa fa-building-o", "", "B8E4B3B0-B543-48B6-93BE-604D4F368559", 0 ); // Room Management

            #endregion

            #region Approval Process

            RockMigrationHelper.UpdateWorkflowType( false, true, "Approval Process", "A workflow that sends an email to the party responsible for the next step in the room reservation approval process.", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Approval Request", "fa fa-list-ol", 28800, true, 0, "83907883-4803-4AFB-8A20-49FDC0BE4788", 0 ); // Approval Process
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Approval State", "ApprovalState", "", 0, @"", "680C5D16-3D16-4DC7-811B-92E272332F0C", true ); // Approval Process:Approval State
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Initial Approval Group", "InitialApprovalGroup", "An optional group that gives initial approval for reservations.", 1, @"", "F48113BA-060A-4BC2-ACD4-56949A32694D", false ); // Approval Process:Initial Approval Group
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Final Approval Group", "FinalApprovalGroup", "An optional group that gives final approval for reservations.", 2, @"", "72993462-A4D1-405F-BE5B-A57625EDEA70", false ); // Approval Process:Final Approval Group
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7", "Reservation", "Reservation", "", 3, @"", "E21D220B-8251-417C-9C8E-91AFD1C677F9", true ); // Approval Process:Reservation
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Requester", "Requester", "", 4, @"", "071BDAC1-6333-47B1-A376-24D97EAA9326", false ); // Approval Process:Requester
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Admin Contact", "AdminContact", "", 5, @"", "35AE460C-60EB-4DC5-98E9-B77B7C8A181C", false ); // Approval Process:Admin Contact
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Event Contact", "EventContact", "", 6, @"", "137CB45C-A1EC-4293-A6F6-0DCFDC40A7C8", false ); // Approval Process:Event Contact
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "FE95430C-322D-4B67-9C77-DFD1D4408725", "Initial Approval Date Time", "InitialApprovalDateTime", "", 7, @"", "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", false ); // Approval Process:Initial Approval Date Time
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "FE95430C-322D-4B67-9C77-DFD1D4408725", "Special Approval Date Time", "SpecialApprovalDateTime", "", 8, @"", "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", false ); // Approval Process:Special Approval Date Time
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "FE95430C-322D-4B67-9C77-DFD1D4408725", "Final Approval Date Time", "FinalApprovalDateTime", "", 9, @"", "43301CCE-CFB2-4CB0-839D-3B9E9006D159", false ); // Approval Process:Final Approval Date Time
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Groups Notified", "GroupsNotified", "", 10, @"", "3A7EC907-D5D4-4DDA-B616-904B17570636", false ); // Approval Process:Groups Notified
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Initial Special Approval Reminders Sent", "InitialSpecialApprovalRemindersSent", "", 11, @"False", "98EF8E62-8094-4799-A7CE-A5D4713E9372", false ); // Approval Process:Initial Special Approval Reminders Sent
            RockMigrationHelper.AddAttributeQualifier( "680C5D16-3D16-4DC7-811B-92E272332F0C", "ispassword", @"False", "245DD12F-91E3-46D5-9C32-33F85F30D653" ); // Approval Process:Approval State:ispassword
            RockMigrationHelper.AddAttributeQualifier( "680C5D16-3D16-4DC7-811B-92E272332F0C", "maxcharacters", @"", "9A444E68-89D7-44B9-B8F2-4F0D895ECE37" ); // Approval Process:Approval State:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "680C5D16-3D16-4DC7-811B-92E272332F0C", "showcountdown", @"False", "00935310-D01E-49DA-8FD1-136EFF064E09" ); // Approval Process:Approval State:showcountdown
            RockMigrationHelper.AddAttributeQualifier( "071BDAC1-6333-47B1-A376-24D97EAA9326", "EnableSelfSelection", @"False", "ACFB371F-E8FE-4641-BA87-6A36480421B9" ); // Approval Process:Requester:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "35AE460C-60EB-4DC5-98E9-B77B7C8A181C", "EnableSelfSelection", @"False", "31B68F22-0EF7-46F2-B8E1-AFE8C4DA4972" ); // Approval Process:Admin Contact:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "137CB45C-A1EC-4293-A6F6-0DCFDC40A7C8", "EnableSelfSelection", @"False", "D61C63CC-7ECA-408D-90F9-DF1F3B9DDB1E" ); // Approval Process:Event Contact:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", "datePickerControlType", @"Date Picker", "99FF21D4-C9E8-45E9-B674-6450C729FA47" ); // Approval Process:Initial Approval Date Time:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", "displayCurrentOption", @"False", "D75EFADF-C87D-4C5F-9741-074373C82F81" ); // Approval Process:Initial Approval Date Time:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", "displayDiff", @"False", "287EBC39-A8BD-41D5-B5C4-73ED2B51550C" ); // Approval Process:Initial Approval Date Time:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", "format", @"", "B44A8F7B-5518-440D-BF06-BDBED683E890" ); // Approval Process:Initial Approval Date Time:format
            RockMigrationHelper.AddAttributeQualifier( "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", "futureYearCount", @"", "95A2738F-3529-4DBC-933D-DC110509FAB2" ); // Approval Process:Initial Approval Date Time:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", "datePickerControlType", @"Date Picker", "537F38B8-F5B6-4A33-9C16-9B93572E5DF2" ); // Approval Process:Special Approval Date Time:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", "displayCurrentOption", @"False", "2398C2C9-B13B-44C9-8C73-3A32057B7D4D" ); // Approval Process:Special Approval Date Time:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", "displayDiff", @"False", "E9A6746C-DE04-4832-9BB7-31CCA6085039" ); // Approval Process:Special Approval Date Time:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", "format", @"", "50EA114D-FF32-4E94-B985-A6BB2D08551D" ); // Approval Process:Special Approval Date Time:format
            RockMigrationHelper.AddAttributeQualifier( "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", "futureYearCount", @"", "B4FB582A-918C-44E5-BE1D-CB5666ECA6AC" ); // Approval Process:Special Approval Date Time:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "43301CCE-CFB2-4CB0-839D-3B9E9006D159", "datePickerControlType", @"Date Picker", "1D2A50A2-0925-42F9-83B1-B79A841460D2" ); // Approval Process:Final Approval Date Time:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "43301CCE-CFB2-4CB0-839D-3B9E9006D159", "displayCurrentOption", @"False", "3530AF3A-3861-4E4A-B0FF-1855F15E133A" ); // Approval Process:Final Approval Date Time:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "43301CCE-CFB2-4CB0-839D-3B9E9006D159", "displayDiff", @"False", "3B16D75D-A8DD-4520-A869-89DECA9BB727" ); // Approval Process:Final Approval Date Time:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "43301CCE-CFB2-4CB0-839D-3B9E9006D159", "format", @"", "8A98E2C2-1711-4176-8D95-0248A87799BD" ); // Approval Process:Final Approval Date Time:format
            RockMigrationHelper.AddAttributeQualifier( "43301CCE-CFB2-4CB0-839D-3B9E9006D159", "futureYearCount", @"", "6DE30B09-8E84-4454-B803-147980983CFA" ); // Approval Process:Final Approval Date Time:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "3A7EC907-D5D4-4DDA-B616-904B17570636", "ispassword", @"False", "76B0B7DA-0C56-40D6-BA03-FADAE809C32A" ); // Approval Process:Groups Notified:ispassword
            RockMigrationHelper.AddAttributeQualifier( "3A7EC907-D5D4-4DDA-B616-904B17570636", "maxcharacters", @"", "A41E095E-A74A-4EDE-B506-900D328C093C" ); // Approval Process:Groups Notified:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "3A7EC907-D5D4-4DDA-B616-904B17570636", "showcountdown", @"False", "23D6FA07-4FEA-4E12-83BF-0557157642C3" ); // Approval Process:Groups Notified:showcountdown
            RockMigrationHelper.AddAttributeQualifier( "98EF8E62-8094-4799-A7CE-A5D4713E9372", "BooleanControlType", @"0", "9FCA77AF-3C7D-4657-93DA-730BE327CAF7" ); // Approval Process:Initial Special Approval Reminders Sent:BooleanControlType
            RockMigrationHelper.AddAttributeQualifier( "98EF8E62-8094-4799-A7CE-A5D4713E9372", "falsetext", @"No", "50001757-815D-45AD-B040-614A1EA365A9" ); // Approval Process:Initial Special Approval Reminders Sent:falsetext
            RockMigrationHelper.AddAttributeQualifier( "98EF8E62-8094-4799-A7CE-A5D4713E9372", "truetext", @"Yes", "3BD4AC40-E154-4FC7-BEDD-621515334714" ); // Approval Process:Initial Special Approval Reminders Sent:truetext
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Set Attributes and Launch State Activity", "", true, 0, "7DE18C65-E54F-4159-86C1-EB9B7AB9733B" ); // Approval Process:Set Attributes and Launch State Activity
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Draft", "", false, 1, "1E06F3F0-0C81-414F-B759-DCF21941E286" ); // Approval Process:Draft
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Pending Initial Approval", "", false, 2, "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04" ); // Approval Process:Pending Initial Approval
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Pending Special Approval", "", false, 3, "C21EF7B1-3B5C-4820-B123-F9241E206E27" ); // Approval Process:Pending Special Approval
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Pending Final Approval", "", false, 4, "7D8E5A78-E443-4657-9B10-39F9F0ADCF15" ); // Approval Process:Pending Final Approval
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Approved", "", false, 5, "F5ECAD7E-8855-4249-A3BB-EFBF626FBA64" ); // Approval Process:Approved
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Changes Needed", "", false, 6, "A9068051-4EE2-4529-9C06-7EFC7E0EF7D7" ); // Approval Process:Changes Needed
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Denied", "", false, 7, "7272A2DF-926C-46E1-87A9-DCD4BC992C67" ); // Approval Process:Denied
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Cancelled", "", false, 8, "1ACA9FB8-4792-4A4E-9EB8-8489E300550E" ); // Approval Process:Cancelled
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Delay Activated", "acd421c1-0f01-4d3f-8d49-c1afc8a81adb", "", 0, @"", "09A25CB0-F0A7-4FBE-BE06-44337D4D096F" ); // Approval Process:Pending Special Approval:Delay Activated
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Initial Approval Group From Entity", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "B37587B4-EECE-479B-BE13-FC7188395CC0" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Group From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Final Approval Group from Entity", 1, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "BB592021-5DC7-4E43-B524-33B018BDDBE6" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Group from Entity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Reservation From Entity", 2, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "04BA3EA4-BD2E-4A71-AE46-E1B910AF82E6" ); // Approval Process:Set Attributes and Launch State Activity:Set Reservation From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Event Contact from Entity", 3, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "230AD55E-0439-4952-80B2-7D0D6A94AAFE" ); // Approval Process:Set Attributes and Launch State Activity:Set Event Contact from Entity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Admin Contact From Entity", 4, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "34AE3A26-0475-4BF5-8111-C1248EF8DC01" ); // Approval Process:Set Attributes and Launch State Activity:Set Admin Contact From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Approval State From Entity", 5, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "E432E227-E78F-4325-A94B-AA4791623C0C" ); // Approval Process:Set Attributes and Launch State Activity:Set Approval State From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Initial Approval Date Time", 6, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "9545A775-4307-4E90-B88F-EFF33BB53557" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Date Time
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Special Approval Date Time", 7, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "245EF97F-6E16-4B9E-B803-5648AA3B7E07" ); // Approval Process:Set Attributes and Launch State Activity:Set Special Approval Date Time
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Final Approval Date Time", 8, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "55AF39A6-51E8-4D5A-95B6-741AA8CBFDF3" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Date Time
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Workflow Name", 9, "36005473-BD5D-470B-B28D-98E6D7ED808D", true, false, "", "", 1, "", "2D61751F-BA93-4DA3-B348-1CB81F25E7EA" ); // Approval Process:Set Attributes and Launch State Activity:Set Workflow Name
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Draft Activity", 10, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "Draft", "1CC7342E-CD69-4CC7-9BBC-FA99BAFA014F" ); // Approval Process:Set Attributes and Launch State Activity:Activate Draft Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Pending Initial Approval Activity", 11, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "PendingInitialApproval", "D8219216-2053-4456-BF91-A79867ADDE5F" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Initial Approval Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Pending Special Approval Activity", 12, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "PendingSpecialApproval", "3385A369-28C9-45A5-86D6-8A10A36F81E4" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Special Approval Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Pending Final Approval Activity", 13, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "PendingFinalApproval", "730125CC-0A1E-4CE5-B782-C4EC50CBAC8F" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Final Approval Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Approved Activity", 14, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "Approved", "C34FE8E8-9E7A-4AD5-823F-A2A7343EE4D4" ); // Approval Process:Set Attributes and Launch State Activity:Activate Approved Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Requires Changes Activity", 15, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "ChangesNeeded", "0635F211-92CD-492F-A932-CEF30CDA5DB3" ); // Approval Process:Set Attributes and Launch State Activity:Activate Requires Changes Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Denied Activity", 16, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "Denied", "0A40E2A1-0C12-49E2-A02A-69645678C031" ); // Approval Process:Set Attributes and Launch State Activity:Activate Denied Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Cancelled Activity", 17, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "Cancelled", "6AF8CB0D-09FE-4D15-B70E-828670B70A72" ); // Approval Process:Set Attributes and Launch State Activity:Activate Cancelled Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Complete Workflow if No Matching Approval State", 18, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "0E6829B8-E61A-44EF-96D7-1713C51F178F" ); // Approval Process:Set Attributes and Launch State Activity:Complete Workflow if No Matching Approval State
            RockMigrationHelper.UpdateWorkflowActionType( "1E06F3F0-0C81-414F-B759-DCF21941E286", "Complete Workflow", 0, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "AF3DBE3B-C511-4970-BB66-8B438F9028E9" ); // Approval Process:Draft:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04", "Set Reservation to Pending Special Approval If No Initial Approval Group", 0, "3894452A-E763-41AC-8260-10373646D8A0", true, false, "", "F48113BA-060A-4BC2-ACD4-56949A32694D", 32, "", "19CB34A3-C5E0-453A-8DE1-7BFE43648439" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If No Initial Approval Group
            RockMigrationHelper.UpdateWorkflowActionType( "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04", "Activate Pending Special Approval Activity If No Initial Approval Group", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "F48113BA-060A-4BC2-ACD4-56949A32694D", 32, "", "E1A0E981-B206-4F68-800A-616CC44B51B0" ); // Approval Process:Pending Initial Approval:Activate Pending Special Approval Activity If No Initial Approval Group
            RockMigrationHelper.UpdateWorkflowActionType( "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04", "Set Reservation to Pending Special Approval If Already Approved", 2, "3894452A-E763-41AC-8260-10373646D8A0", true, false, "", "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", 64, "", "284044DD-DD22-43CB-A540-A7297C89843D" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If Already Approved
            RockMigrationHelper.UpdateWorkflowActionType( "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04", "Activate Pending Special Approval Activity If Already Approved", 3, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", 64, "", "DC2E6CC2-DF0F-4600-9687-9911A35698DC" ); // Approval Process:Pending Initial Approval:Activate Pending Special Approval Activity If Already Approved
            RockMigrationHelper.UpdateWorkflowActionType( "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04", "Send Email", 4, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "F48113BA-060A-4BC2-ACD4-56949A32694D", 64, "", "F783EE15-AB52-49EA-AD16-BDCFABB1A2D2" ); // Approval Process:Pending Initial Approval:Send Email
            RockMigrationHelper.UpdateWorkflowActionType( "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04", "Complete Workflow", 5, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "72993462-A4D1-405F-BE5B-A57625EDEA70", 64, "", "F8F6ECEA-728F-4ABB-B02A-D69D45B09388" ); // Approval Process:Pending Initial Approval:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Delay 14 Days if Notifications Already Sent", 0, "D22E73F7-86E2-46CA-AD5B-7770A866726B", true, false, "", "98EF8E62-8094-4799-A7CE-A5D4713E9372", 1, "Yes", "ACD421C1-0F01-4D3F-8D49-C1AFC8A81ADB" ); // Approval Process:Pending Special Approval:Delay 14 Days if Notifications Already Sent
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Set Resource States", 1, "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", true, false, "", "", 1, "", "39FBCAFE-4594-4D5A-9111-C37E8011C76C" ); // Approval Process:Pending Special Approval:Set Resource States
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Set Location States", 2, "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", true, false, "", "", 1, "", "664A5031-3B33-4FD8-9BBA-C16E5A644ADF" ); // Approval Process:Pending Special Approval:Set Location States
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Send Notifications to Any Special Approval Groups", 3, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "DB6E99A9-D680-48B7-BCFF-5DDF88D3DEB5" ); // Approval Process:Pending Special Approval:Send Notifications to Any Special Approval Groups
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Mark Initial Notifications as Sent", 4, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "D0D12B3F-A5C3-4DE4-9D10-B44AD4ED0573" ); // Approval Process:Pending Special Approval:Mark Initial Notifications as Sent
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Complete Workflow If Approval State Has Changed Since Activity Started", 5, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "3A7EC907-D5D4-4DDA-B616-904B17570636", 512, "0", "E9EA7248-69AD-4C71-9A90-342E55D2739A" ); // Approval Process:Pending Special Approval:Complete Workflow If Approval State Has Changed Since Activity Started
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Activate Send Special Approval Reminders if Any Special Approval Groups", 6, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "3A7EC907-D5D4-4DDA-B616-904B17570636", 128, "0", "C7A536B5-DE10-4B0E-B02B-3C02C8306C04" ); // Approval Process:Pending Special Approval:Activate Send Special Approval Reminders if Any Special Approval Groups
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Set Special Approval Date If Blank", 7, "C545211C-1143-498E-8B3A-FEE9D59C7C96", true, false, "", "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", 32, "", "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Set Reservation to Pending Final Approval If No Special Approval Groups", 8, "3894452A-E763-41AC-8260-10373646D8A0", true, false, "", "3A7EC907-D5D4-4DDA-B616-904B17570636", 1, "0", "665A49A2-5ED1-4994-A348-CDD6F9CC184A" ); // Approval Process:Pending Special Approval:Set Reservation to Pending Final Approval If No Special Approval Groups
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Activate Pending Final Approval Activity If No Special Approval Groups", 9, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "3A7EC907-D5D4-4DDA-B616-904B17570636", 1, "0", "645B7181-936B-460C-B577-C4441781FA04" ); // Approval Process:Pending Special Approval:Activate Pending Final Approval Activity If No Special Approval Groups
            RockMigrationHelper.UpdateWorkflowActionType( "7D8E5A78-E443-4657-9B10-39F9F0ADCF15", "Set Reservation to Approved If No Final Approval Group", 0, "3894452A-E763-41AC-8260-10373646D8A0", true, false, "", "72993462-A4D1-405F-BE5B-A57625EDEA70", 32, "", "4B4BD44D-A122-44CF-B097-458E2DE91E78" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If No Final Approval Group
            RockMigrationHelper.UpdateWorkflowActionType( "7D8E5A78-E443-4657-9B10-39F9F0ADCF15", "Activate Approved Activity If No Final Approval Group", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "72993462-A4D1-405F-BE5B-A57625EDEA70", 32, "", "F66AB76B-78E2-4651-A10A-C7D60E37869B" ); // Approval Process:Pending Final Approval:Activate Approved Activity If No Final Approval Group
            RockMigrationHelper.UpdateWorkflowActionType( "7D8E5A78-E443-4657-9B10-39F9F0ADCF15", "Set Reservation to Approved If Already Approved", 2, "3894452A-E763-41AC-8260-10373646D8A0", true, false, "", "43301CCE-CFB2-4CB0-839D-3B9E9006D159", 64, "", "2B7E16A5-3702-4129-8FD4-702CFCCD7F18" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If Already Approved
            RockMigrationHelper.UpdateWorkflowActionType( "7D8E5A78-E443-4657-9B10-39F9F0ADCF15", "Activate Approved Activity If Already Approved", 3, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "43301CCE-CFB2-4CB0-839D-3B9E9006D159", 64, "", "32B47E6E-3CEC-48CB-B074-D78CDE283CDD" ); // Approval Process:Pending Final Approval:Activate Approved Activity If Already Approved
            RockMigrationHelper.UpdateWorkflowActionType( "7D8E5A78-E443-4657-9B10-39F9F0ADCF15", "Send Email", 4, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "72993462-A4D1-405F-BE5B-A57625EDEA70", 64, "", "E13B02B1-0BA4-4F23-AE58-01E822406522" ); // Approval Process:Pending Final Approval:Send Email
            RockMigrationHelper.UpdateWorkflowActionType( "7D8E5A78-E443-4657-9B10-39F9F0ADCF15", "Complete Workflow", 5, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "72993462-A4D1-405F-BE5B-A57625EDEA70", 64, "", "1EEE73A3-7E3A-4FCE-B241-827FB929AF89" ); // Approval Process:Pending Final Approval:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "F5ECAD7E-8855-4249-A3BB-EFBF626FBA64", "Set Location States", 0, "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", true, false, "", "", 1, "", "20A1F5F6-9612-4BE1-B2B6-94A2424AC060" ); // Approval Process:Approved:Set Location States
            RockMigrationHelper.UpdateWorkflowActionType( "F5ECAD7E-8855-4249-A3BB-EFBF626FBA64", "Set Resource States", 1, "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", true, false, "", "", 1, "", "EEB50C19-FF94-408E-A456-4B875655A23E" ); // Approval Process:Approved:Set Resource States
            RockMigrationHelper.UpdateWorkflowActionType( "F5ECAD7E-8855-4249-A3BB-EFBF626FBA64", "Send Email to Admin Contact", 2, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "AFA4DBFB-2581-4004-AA01-6FF621C4725C" ); // Approval Process:Approved:Send Email to Admin Contact
            RockMigrationHelper.UpdateWorkflowActionType( "F5ECAD7E-8855-4249-A3BB-EFBF626FBA64", "Send Email to Event Contact", 3, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "969CD77B-022A-4518-A0A6-2C19D3ED3A65" ); // Approval Process:Approved:Send Email to Event Contact
            RockMigrationHelper.UpdateWorkflowActionType( "F5ECAD7E-8855-4249-A3BB-EFBF626FBA64", "Complete Workflow", 4, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "9C0CCC1F-1E1F-4879-89E9-AA34B5DB9B80" ); // Approval Process:Approved:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "A9068051-4EE2-4529-9C06-7EFC7E0EF7D7", "Send Email to Admin Contact", 0, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "9DB9F652-5EA4-483E-B922-3530D1188379" ); // Approval Process:Changes Needed:Send Email to Admin Contact
            RockMigrationHelper.UpdateWorkflowActionType( "A9068051-4EE2-4529-9C06-7EFC7E0EF7D7", "Send Email to Event Contact", 1, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "A31A317A-8F98-4D94-866A-C56505BF6756" ); // Approval Process:Changes Needed:Send Email to Event Contact
            RockMigrationHelper.UpdateWorkflowActionType( "A9068051-4EE2-4529-9C06-7EFC7E0EF7D7", "Complete Workflow", 2, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "6A1E6886-D4BA-48C2-A860-D067AADAC8CE" ); // Approval Process:Changes Needed:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "7272A2DF-926C-46E1-87A9-DCD4BC992C67", "Set Location States", 0, "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", true, false, "", "", 1, "", "5336AC78-3B7D-4229-99B0-09E8D43ED058" ); // Approval Process:Denied:Set Location States
            RockMigrationHelper.UpdateWorkflowActionType( "7272A2DF-926C-46E1-87A9-DCD4BC992C67", "Set Resource States", 1, "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", true, false, "", "", 1, "", "69F1D448-C818-4A9F-B01D-C136D6D9275A" ); // Approval Process:Denied:Set Resource States
            RockMigrationHelper.UpdateWorkflowActionType( "7272A2DF-926C-46E1-87A9-DCD4BC992C67", "Send Email to Admin Contact", 2, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "27DA6AF4-DFAC-4D72-B068-FB5C5B4BD9A4" ); // Approval Process:Denied:Send Email to Admin Contact
            RockMigrationHelper.UpdateWorkflowActionType( "7272A2DF-926C-46E1-87A9-DCD4BC992C67", "Send Email to Event Contact", 3, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "79551640-080B-44F9-A00B-1AFB1A6D3AD7" ); // Approval Process:Denied:Send Email to Event Contact
            RockMigrationHelper.UpdateWorkflowActionType( "7272A2DF-926C-46E1-87A9-DCD4BC992C67", "Complete Workflow", 4, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "22043E5A-45F7-41C4-93AF-9308B1222291" ); // Approval Process:Denied:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "1ACA9FB8-4792-4A4E-9EB8-8489E300550E", "Complete Workflow", 0, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "D4B2E0BB-6FC7-4B68-AF08-72B4A8CD7C7E" ); // Approval Process:Cancelled:Complete Workflow
            RockMigrationHelper.AddActionTypeAttributeValue( "B37587B4-EECE-479B-BE13-FC7188395CC0", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Group From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B37587B4-EECE-479B-BE13-FC7188395CC0", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"f48113ba-060a-4bc2-acd4-56949a32694d" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Group From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "B37587B4-EECE-479B-BE13-FC7188395CC0", "B524B00C-29CB-49E9-9896-8BB60F209783", @"True" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Group From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "B37587B4-EECE-479B-BE13-FC7188395CC0", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Group From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "B37587B4-EECE-479B-BE13-FC7188395CC0", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199", @"{{ Entity.ReservationType.InitialApprovalGroup.Guid }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Group From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "BB592021-5DC7-4E43-B524-33B018BDDBE6", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Group from Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "BB592021-5DC7-4E43-B524-33B018BDDBE6", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"72993462-a4d1-405f-be5b-a57625edea70" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Group from Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "BB592021-5DC7-4E43-B524-33B018BDDBE6", "B524B00C-29CB-49E9-9896-8BB60F209783", @"True" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Group from Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "BB592021-5DC7-4E43-B524-33B018BDDBE6", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Group from Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "BB592021-5DC7-4E43-B524-33B018BDDBE6", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199", @"{{ Entity.ReservationType.FinalApprovalGroup.Guid }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Group from Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "04BA3EA4-BD2E-4A71-AE46-E1B910AF82E6", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Reservation From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "04BA3EA4-BD2E-4A71-AE46-E1B910AF82E6", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Set Attributes and Launch State Activity:Set Reservation From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "04BA3EA4-BD2E-4A71-AE46-E1B910AF82E6", "B524B00C-29CB-49E9-9896-8BB60F209783", @"True" ); // Approval Process:Set Attributes and Launch State Activity:Set Reservation From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "04BA3EA4-BD2E-4A71-AE46-E1B910AF82E6", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Reservation From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "04BA3EA4-BD2E-4A71-AE46-E1B910AF82E6", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199", @"{{ Entity.Guid }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Reservation From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "230AD55E-0439-4952-80B2-7D0D6A94AAFE", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Event Contact from Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "230AD55E-0439-4952-80B2-7D0D6A94AAFE", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"137cb45c-a1ec-4293-a6f6-0dcfdc40a7c8" ); // Approval Process:Set Attributes and Launch State Activity:Set Event Contact from Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "230AD55E-0439-4952-80B2-7D0D6A94AAFE", "B524B00C-29CB-49E9-9896-8BB60F209783", @"True" ); // Approval Process:Set Attributes and Launch State Activity:Set Event Contact from Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "230AD55E-0439-4952-80B2-7D0D6A94AAFE", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Event Contact from Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "230AD55E-0439-4952-80B2-7D0D6A94AAFE", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199", @"{{ Entity.EventContactPersonAlias.Guid }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Event Contact from Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "34AE3A26-0475-4BF5-8111-C1248EF8DC01", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Admin Contact From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "34AE3A26-0475-4BF5-8111-C1248EF8DC01", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"35ae460c-60eb-4dc5-98e9-b77b7c8a181c" ); // Approval Process:Set Attributes and Launch State Activity:Set Admin Contact From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "34AE3A26-0475-4BF5-8111-C1248EF8DC01", "B524B00C-29CB-49E9-9896-8BB60F209783", @"True" ); // Approval Process:Set Attributes and Launch State Activity:Set Admin Contact From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "34AE3A26-0475-4BF5-8111-C1248EF8DC01", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Admin Contact From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "34AE3A26-0475-4BF5-8111-C1248EF8DC01", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199", @"{{ Entity.AdministrativeContactPersonAlias.Guid }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Admin Contact From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "E432E227-E78F-4325-A94B-AA4791623C0C", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Approval State From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E432E227-E78F-4325-A94B-AA4791623C0C", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"680c5d16-3d16-4dc7-811b-92e272332f0c" ); // Approval Process:Set Attributes and Launch State Activity:Set Approval State From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "E432E227-E78F-4325-A94B-AA4791623C0C", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"{% assign reservation =  Workflow | Attribute:'Reservation', 'object' %}{{ reservation.ApprovalState }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Approval State From Entity:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "9545A775-4307-4E90-B88F-EFF33BB53557", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Date Time:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9545A775-4307-4E90-B88F-EFF33BB53557", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"b85c25fd-2a6d-4402-9f86-25d9c3179a8e" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Date Time:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "9545A775-4307-4E90-B88F-EFF33BB53557", "B524B00C-29CB-49E9-9896-8BB60F209783", @"True" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Date Time:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "9545A775-4307-4E90-B88F-EFF33BB53557", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Date Time:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "9545A775-4307-4E90-B88F-EFF33BB53557", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199", @"{{ Entity.InitialApprovalDateTime }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Date Time:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "245EF97F-6E16-4B9E-B803-5648AA3B7E07", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Special Approval Date Time:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "245EF97F-6E16-4B9E-B803-5648AA3B7E07", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"a2b592ba-9c64-4faf-aadd-df2d12d12989" ); // Approval Process:Set Attributes and Launch State Activity:Set Special Approval Date Time:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "245EF97F-6E16-4B9E-B803-5648AA3B7E07", "B524B00C-29CB-49E9-9896-8BB60F209783", @"True" ); // Approval Process:Set Attributes and Launch State Activity:Set Special Approval Date Time:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "245EF97F-6E16-4B9E-B803-5648AA3B7E07", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Special Approval Date Time:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "245EF97F-6E16-4B9E-B803-5648AA3B7E07", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199", @"{{ Entity.SpecialApprovalDateTime }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Special Approval Date Time:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "55AF39A6-51E8-4D5A-95B6-741AA8CBFDF3", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Date Time:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "55AF39A6-51E8-4D5A-95B6-741AA8CBFDF3", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"43301cce-cfb2-4cb0-839d-3b9e9006d159" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Date Time:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "55AF39A6-51E8-4D5A-95B6-741AA8CBFDF3", "B524B00C-29CB-49E9-9896-8BB60F209783", @"True" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Date Time:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "55AF39A6-51E8-4D5A-95B6-741AA8CBFDF3", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Date Time:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "55AF39A6-51E8-4D5A-95B6-741AA8CBFDF3", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199", @"{{ Entity.FinalApprovalDateTime }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Date Time:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "2D61751F-BA93-4DA3-B348-1CB81F25E7EA", "0A800013-51F7-4902-885A-5BE215D67D3D", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Workflow Name:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2D61751F-BA93-4DA3-B348-1CB81F25E7EA", "93852244-A667-4749-961A-D47F88675BE4", @"{{Workflow | Attribute:'Reservation','Name'}} (ID:{{Workflow | Attribute:'Reservation','Id'}}): {{Workflow | Attribute:'ApprovalState'}}" ); // Approval Process:Set Attributes and Launch State Activity:Set Workflow Name:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "1CC7342E-CD69-4CC7-9BBC-FA99BAFA014F", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Draft Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1CC7342E-CD69-4CC7-9BBC-FA99BAFA014F", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"1E06F3F0-0C81-414F-B759-DCF21941E286" ); // Approval Process:Set Attributes and Launch State Activity:Activate Draft Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "D8219216-2053-4456-BF91-A79867ADDE5F", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Initial Approval Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D8219216-2053-4456-BF91-A79867ADDE5F", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Initial Approval Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "3385A369-28C9-45A5-86D6-8A10A36F81E4", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Special Approval Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3385A369-28C9-45A5-86D6-8A10A36F81E4", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"C21EF7B1-3B5C-4820-B123-F9241E206E27" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Special Approval Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "730125CC-0A1E-4CE5-B782-C4EC50CBAC8F", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Final Approval Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "730125CC-0A1E-4CE5-B782-C4EC50CBAC8F", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"7D8E5A78-E443-4657-9B10-39F9F0ADCF15" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Final Approval Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "C34FE8E8-9E7A-4AD5-823F-A2A7343EE4D4", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Approved Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C34FE8E8-9E7A-4AD5-823F-A2A7343EE4D4", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"F5ECAD7E-8855-4249-A3BB-EFBF626FBA64" ); // Approval Process:Set Attributes and Launch State Activity:Activate Approved Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "0635F211-92CD-492F-A932-CEF30CDA5DB3", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Requires Changes Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0635F211-92CD-492F-A932-CEF30CDA5DB3", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"A9068051-4EE2-4529-9C06-7EFC7E0EF7D7" ); // Approval Process:Set Attributes and Launch State Activity:Activate Requires Changes Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "0A40E2A1-0C12-49E2-A02A-69645678C031", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Denied Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0A40E2A1-0C12-49E2-A02A-69645678C031", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"7272A2DF-926C-46E1-87A9-DCD4BC992C67" ); // Approval Process:Set Attributes and Launch State Activity:Activate Denied Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "6AF8CB0D-09FE-4D15-B70E-828670B70A72", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Cancelled Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6AF8CB0D-09FE-4D15-B70E-828670B70A72", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"1ACA9FB8-4792-4A4E-9EB8-8489E300550E" ); // Approval Process:Set Attributes and Launch State Activity:Activate Cancelled Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "0E6829B8-E61A-44EF-96D7-1713C51F178F", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Complete Workflow if No Matching Approval State:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0E6829B8-E61A-44EF-96D7-1713C51F178F", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Set Attributes and Launch State Activity:Complete Workflow if No Matching Approval State:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "AF3DBE3B-C511-4970-BB66-8B438F9028E9", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Draft:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "AF3DBE3B-C511-4970-BB66-8B438F9028E9", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Draft:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "19CB34A3-C5E0-453A-8DE1-7BFE43648439", "ACA008E2-2406-457E-8E4C-6922E03757A4", @"False" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If No Initial Approval Group:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "19CB34A3-C5E0-453A-8DE1-7BFE43648439", "1D4F819F-145D-4A7F-AB4E-AD7C06759042", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If No Initial Approval Group:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "19CB34A3-C5E0-453A-8DE1-7BFE43648439", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2", @"6" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If No Initial Approval Group:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "E1A0E981-B206-4F68-800A-616CC44B51B0", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Initial Approval:Activate Pending Special Approval Activity If No Initial Approval Group:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E1A0E981-B206-4F68-800A-616CC44B51B0", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Initial Approval:Activate Pending Special Approval Activity If No Initial Approval Group:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "284044DD-DD22-43CB-A540-A7297C89843D", "ACA008E2-2406-457E-8E4C-6922E03757A4", @"False" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If Already Approved:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "284044DD-DD22-43CB-A540-A7297C89843D", "1D4F819F-145D-4A7F-AB4E-AD7C06759042", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If Already Approved:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "284044DD-DD22-43CB-A540-A7297C89843D", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2", @"6" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If Already Approved:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "DC2E6CC2-DF0F-4600-9687-9911A35698DC", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Initial Approval:Activate Pending Special Approval Activity If Already Approved:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "DC2E6CC2-DF0F-4600-9687-9911A35698DC", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Initial Approval:Activate Pending Special Approval Activity If Already Approved:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "F783EE15-AB52-49EA-AD16-BDCFABB1A2D2", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Pending Initial Approval:Send Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F783EE15-AB52-49EA-AD16-BDCFABB1A2D2", "0C4C13B8-7076-4872-925A-F950886B5E16", @"f48113ba-060a-4bc2-acd4-56949a32694d" ); // Approval Process:Pending Initial Approval:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "F783EE15-AB52-49EA-AD16-BDCFABB1A2D2", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Initial Approval Needed: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Pending Initial Approval:Send Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "F783EE15-AB52-49EA-AD16-BDCFABB1A2D2", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
A new reservation requires your initial approval:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Pending Initial Approval:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "F783EE15-AB52-49EA-AD16-BDCFABB1A2D2", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Pending Initial Approval:Send Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "F8F6ECEA-728F-4ABB-B02A-D69D45B09388", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Initial Approval:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F8F6ECEA-728F-4ABB-B02A-D69D45B09388", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Initial Approval:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "ACD421C1-0F01-4D3F-8D49-C1AFC8A81ADB", "D916CB65-9413-479F-8F5E-6E599CE48025", @"False" ); // Approval Process:Pending Special Approval:Delay 14 Days if Notifications Already Sent:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "ACD421C1-0F01-4D3F-8D49-C1AFC8A81ADB", "3C501BE2-FE9E-479D-BC59-8F3B72FF6E4A", @"20160" ); // Approval Process:Pending Special Approval:Delay 14 Days if Notifications Already Sent:Minutes To Delay
            RockMigrationHelper.AddActionTypeAttributeValue( "39FBCAFE-4594-4D5A-9111-C37E8011C76C", "4FBD0A05-603C-4F7E-B122-2865F31A4AD0", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Special Approval:Set Resource States:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "39FBCAFE-4594-4D5A-9111-C37E8011C76C", "C04B4BA1-65E6-493F-BD1B-834D6A1961E8", @"False" ); // Approval Process:Pending Special Approval:Set Resource States:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "39FBCAFE-4594-4D5A-9111-C37E8011C76C", "0E07B013-2C74-4CD7-AFBC-C535151AA60A", @"2" ); // Approval Process:Pending Special Approval:Set Resource States:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "39FBCAFE-4594-4D5A-9111-C37E8011C76C", "3CA5B94F-5571-497C-85EF-2586B62BDFC2", @"True" ); // Approval Process:Pending Special Approval:Set Resource States:Ignore Resources With Approval Groups
            RockMigrationHelper.AddActionTypeAttributeValue( "664A5031-3B33-4FD8-9BBA-C16E5A644ADF", "D530F451-D1FA-416A-86F1-7E87FBCC4EC7", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Special Approval:Set Location States:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "664A5031-3B33-4FD8-9BBA-C16E5A644ADF", "15A2EEA8-53DA-4ADD-9DDE-FF2FCA70023F", @"False" ); // Approval Process:Pending Special Approval:Set Location States:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "664A5031-3B33-4FD8-9BBA-C16E5A644ADF", "53B06E3D-22CD-488B-86D3-4C4D81511334", @"2" ); // Approval Process:Pending Special Approval:Set Location States:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "664A5031-3B33-4FD8-9BBA-C16E5A644ADF", "22FAA8A0-DE01-4402-B0A6-89A5C58B180A", @"True" ); // Approval Process:Pending Special Approval:Set Location States:Ignore Locations With Approval Groups
            RockMigrationHelper.AddActionTypeAttributeValue( "DB6E99A9-D680-48B7-BCFF-5DDF88D3DEB5", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign groupsNotified = 0 %}
{% assign reservationId = Workflow | Attribute:'Reservation','Id' %}
{% reservation id:'{{reservationId}}' %}
    {% if reservation.ApprovalState == 'PendingSpecialApproval' %}
        {% for reservationLocation in reservation.ReservationLocations %}
            {% if reservationLocation.ApprovalState == 'Unapproved' %}
                {% assign approvalGroup = null %}
                {% assign approvalGroup = reservationLocation.Location | Attribute:'ApprovalGroup','Object' %}
                {% if approvalGroup != empty %}
                    {% workflowactivate workflowtype:'66899922-D665-4839-8742-BD8556D7FB61' ApprovalGroup:'{{approvalGroup.Guid}}' Reservation:'{{ reservation.Guid }}' RelevantItem:'{{reservationLocation.Location.Name}}' %}
                      {% assign groupsNotified = groupsNotified | Plus:1 %}
                    {% endworkflowactivate %}
                {% endif %}
            {% endif %}
        {% endfor %}
        {% for reservationResource in reservation.ReservationResources %}
            {% if reservationResource.ApprovalState == 'Unapproved' %}
                {% assign approvalGroup = null %}
                {% assign approvalGroup = reservationResource.Resource.ApprovalGroup %}
                {% if approvalGroup != null %}
                    {% workflowactivate workflowtype:'66899922-D665-4839-8742-BD8556D7FB61' ApprovalGroup:'{{approvalGroup.Guid}}' Reservation:'{{ reservation.Guid }}' RelevantItem:'{{reservationResource.Resource.Name}}' %}
                      {% assign groupsNotified = groupsNotified | Plus:1 %}
                    {% endworkflowactivate %}
                {% endif %}
            {% endif %}
        {% endfor %}
    {% else %}
        {% assign groupsNotified = -1 %}
    {% endif %}
{% endreservation %}

{{groupsNotified}}" ); // Approval Process:Pending Special Approval:Send Notifications to Any Special Approval Groups:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "DB6E99A9-D680-48B7-BCFF-5DDF88D3DEB5", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Approval Process:Pending Special Approval:Send Notifications to Any Special Approval Groups:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "DB6E99A9-D680-48B7-BCFF-5DDF88D3DEB5", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"3a7ec907-d5d4-4dda-b616-904b17570636" ); // Approval Process:Pending Special Approval:Send Notifications to Any Special Approval Groups:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "DB6E99A9-D680-48B7-BCFF-5DDF88D3DEB5", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"All" ); // Approval Process:Pending Special Approval:Send Notifications to Any Special Approval Groups:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "D0D12B3F-A5C3-4DE4-9D10-B44AD4ED0573", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Approval Process:Pending Special Approval:Mark Initial Notifications as Sent:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D0D12B3F-A5C3-4DE4-9D10-B44AD4ED0573", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"98ef8e62-8094-4799-a7ce-a5d4713e9372" ); // Approval Process:Pending Special Approval:Mark Initial Notifications as Sent:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "D0D12B3F-A5C3-4DE4-9D10-B44AD4ED0573", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"Yes" ); // Approval Process:Pending Special Approval:Mark Initial Notifications as Sent:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "E9EA7248-69AD-4C71-9A90-342E55D2739A", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Special Approval:Complete Workflow If Approval State Has Changed Since Activity Started:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E9EA7248-69AD-4C71-9A90-342E55D2739A", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Special Approval:Complete Workflow If Approval State Has Changed Since Activity Started:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "C7A536B5-DE10-4B0E-B02B-3C02C8306C04", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Pending Special Approval:Activate Send Special Approval Reminders if Any Special Approval Groups:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C7A536B5-DE10-4B0E-B02B-3C02C8306C04", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"C21EF7B1-3B5C-4820-B123-F9241E206E27" ); // Approval Process:Pending Special Approval:Activate Send Special Approval Reminders if Any Special Approval Groups:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4", "FCA61786-8EC0-44D7-8A3D-152721FF2353", @"False" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4", "8373A55C-E023-4DE0-B583-06FF906520FC", @"839768a3-10d6-446c-a65b-b8f9efd7808f" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank:Entity Type
            RockMigrationHelper.AddActionTypeAttributeValue( "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4", "913D7A95-BC44-4874-92F9-66DB85DF9FEF", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4", "EF2CEB92-D90D-4533-9A1A-3F61E0E436A5", @"SpecialApprovalDateTime" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank:Property Name|Property Name Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4", "0415C959-BF89-4D19-9C47-3AB1098E1FBA", @"{{ 'Now' | Date }}" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank:Property Value|Property Value Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4", "6C6D7AF8-4A4C-46E7-9F57-F150186B7D2C", @"IGNORE" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank:Empty Value Handling
            RockMigrationHelper.AddActionTypeAttributeValue( "665A49A2-5ED1-4994-A348-CDD6F9CC184A", "ACA008E2-2406-457E-8E4C-6922E03757A4", @"False" ); // Approval Process:Pending Special Approval:Set Reservation to Pending Final Approval If No Special Approval Groups:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "665A49A2-5ED1-4994-A348-CDD6F9CC184A", "1D4F819F-145D-4A7F-AB4E-AD7C06759042", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Special Approval:Set Reservation to Pending Final Approval If No Special Approval Groups:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "665A49A2-5ED1-4994-A348-CDD6F9CC184A", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2", @"5" ); // Approval Process:Pending Special Approval:Set Reservation to Pending Final Approval If No Special Approval Groups:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "645B7181-936B-460C-B577-C4441781FA04", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Special Approval:Activate Pending Final Approval Activity If No Special Approval Groups:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "645B7181-936B-460C-B577-C4441781FA04", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Special Approval:Activate Pending Final Approval Activity If No Special Approval Groups:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4B4BD44D-A122-44CF-B097-458E2DE91E78", "ACA008E2-2406-457E-8E4C-6922E03757A4", @"False" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If No Final Approval Group:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4B4BD44D-A122-44CF-B097-458E2DE91E78", "1D4F819F-145D-4A7F-AB4E-AD7C06759042", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If No Final Approval Group:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4B4BD44D-A122-44CF-B097-458E2DE91E78", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2", @"2" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If No Final Approval Group:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "F66AB76B-78E2-4651-A10A-C7D60E37869B", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Final Approval:Activate Approved Activity If No Final Approval Group:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F66AB76B-78E2-4651-A10A-C7D60E37869B", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Final Approval:Activate Approved Activity If No Final Approval Group:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "2B7E16A5-3702-4129-8FD4-702CFCCD7F18", "ACA008E2-2406-457E-8E4C-6922E03757A4", @"False" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If Already Approved:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2B7E16A5-3702-4129-8FD4-702CFCCD7F18", "1D4F819F-145D-4A7F-AB4E-AD7C06759042", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If Already Approved:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "2B7E16A5-3702-4129-8FD4-702CFCCD7F18", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2", @"2" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If Already Approved:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "32B47E6E-3CEC-48CB-B074-D78CDE283CDD", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Final Approval:Activate Approved Activity If Already Approved:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "32B47E6E-3CEC-48CB-B074-D78CDE283CDD", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Final Approval:Activate Approved Activity If Already Approved:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "E13B02B1-0BA4-4F23-AE58-01E822406522", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Pending Final Approval:Send Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E13B02B1-0BA4-4F23-AE58-01E822406522", "0C4C13B8-7076-4872-925A-F950886B5E16", @"72993462-a4d1-405f-be5b-a57625edea70" ); // Approval Process:Pending Final Approval:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "E13B02B1-0BA4-4F23-AE58-01E822406522", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Final Approval Needed: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Pending Final Approval:Send Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "E13B02B1-0BA4-4F23-AE58-01E822406522", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
A new reservation requires your final approval:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Pending Final Approval:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "E13B02B1-0BA4-4F23-AE58-01E822406522", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Pending Final Approval:Send Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "1EEE73A3-7E3A-4FCE-B241-827FB929AF89", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Final Approval:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1EEE73A3-7E3A-4FCE-B241-827FB929AF89", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Final Approval:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "20A1F5F6-9612-4BE1-B2B6-94A2424AC060", "D530F451-D1FA-416A-86F1-7E87FBCC4EC7", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Approved:Set Location States:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "20A1F5F6-9612-4BE1-B2B6-94A2424AC060", "15A2EEA8-53DA-4ADD-9DDE-FF2FCA70023F", @"False" ); // Approval Process:Approved:Set Location States:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "20A1F5F6-9612-4BE1-B2B6-94A2424AC060", "53B06E3D-22CD-488B-86D3-4C4D81511334", @"2" ); // Approval Process:Approved:Set Location States:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "20A1F5F6-9612-4BE1-B2B6-94A2424AC060", "22FAA8A0-DE01-4402-B0A6-89A5C58B180A", @"False" ); // Approval Process:Approved:Set Location States:Ignore Locations With Approval Groups
            RockMigrationHelper.AddActionTypeAttributeValue( "EEB50C19-FF94-408E-A456-4B875655A23E", "4FBD0A05-603C-4F7E-B122-2865F31A4AD0", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Approved:Set Resource States:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "EEB50C19-FF94-408E-A456-4B875655A23E", "C04B4BA1-65E6-493F-BD1B-834D6A1961E8", @"False" ); // Approval Process:Approved:Set Resource States:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "EEB50C19-FF94-408E-A456-4B875655A23E", "0E07B013-2C74-4CD7-AFBC-C535151AA60A", @"2" ); // Approval Process:Approved:Set Resource States:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "EEB50C19-FF94-408E-A456-4B875655A23E", "3CA5B94F-5571-497C-85EF-2586B62BDFC2", @"False" ); // Approval Process:Approved:Set Resource States:Ignore Resources With Approval Groups
            RockMigrationHelper.AddActionTypeAttributeValue( "AFA4DBFB-2581-4004-AA01-6FF621C4725C", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Approved:Send Email to Admin Contact:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "AFA4DBFB-2581-4004-AA01-6FF621C4725C", "0C4C13B8-7076-4872-925A-F950886B5E16", @"35ae460c-60eb-4dc5-98e9-b77b7c8a181c" ); // Approval Process:Approved:Send Email to Admin Contact:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "AFA4DBFB-2581-4004-AA01-6FF621C4725C", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Reservation Approved: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Approved:Send Email to Admin Contact:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "AFA4DBFB-2581-4004-AA01-6FF621C4725C", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
Your reservation has been approved:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Approved:Send Email to Admin Contact:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "AFA4DBFB-2581-4004-AA01-6FF621C4725C", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Approved:Send Email to Admin Contact:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "969CD77B-022A-4518-A0A6-2C19D3ED3A65", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Approved:Send Email to Event Contact:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "969CD77B-022A-4518-A0A6-2C19D3ED3A65", "0C4C13B8-7076-4872-925A-F950886B5E16", @"137cb45c-a1ec-4293-a6f6-0dcfdc40a7c8" ); // Approval Process:Approved:Send Email to Event Contact:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "969CD77B-022A-4518-A0A6-2C19D3ED3A65", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Reservation Approved: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Approved:Send Email to Event Contact:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "969CD77B-022A-4518-A0A6-2C19D3ED3A65", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
Your reservation has been approved:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Approved:Send Email to Event Contact:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "969CD77B-022A-4518-A0A6-2C19D3ED3A65", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Approved:Send Email to Event Contact:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "9C0CCC1F-1E1F-4879-89E9-AA34B5DB9B80", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Approved:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9C0CCC1F-1E1F-4879-89E9-AA34B5DB9B80", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Approved:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "9DB9F652-5EA4-483E-B922-3530D1188379", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Changes Needed:Send Email to Admin Contact:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9DB9F652-5EA4-483E-B922-3530D1188379", "0C4C13B8-7076-4872-925A-F950886B5E16", @"35ae460c-60eb-4dc5-98e9-b77b7c8a181c" ); // Approval Process:Changes Needed:Send Email to Admin Contact:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "9DB9F652-5EA4-483E-B922-3530D1188379", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Changes Needed: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Changes Needed:Send Email to Admin Contact:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "9DB9F652-5EA4-483E-B922-3530D1188379", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
Your reservation requires changes:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Changes Needed:Send Email to Admin Contact:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "9DB9F652-5EA4-483E-B922-3530D1188379", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Changes Needed:Send Email to Admin Contact:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "A31A317A-8F98-4D94-866A-C56505BF6756", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Changes Needed:Send Email to Event Contact:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A31A317A-8F98-4D94-866A-C56505BF6756", "0C4C13B8-7076-4872-925A-F950886B5E16", @"137cb45c-a1ec-4293-a6f6-0dcfdc40a7c8" ); // Approval Process:Changes Needed:Send Email to Event Contact:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "A31A317A-8F98-4D94-866A-C56505BF6756", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Changes Needed: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Changes Needed:Send Email to Event Contact:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "A31A317A-8F98-4D94-866A-C56505BF6756", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
Your reservation requires changes:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Changes Needed:Send Email to Event Contact:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "A31A317A-8F98-4D94-866A-C56505BF6756", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Changes Needed:Send Email to Event Contact:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "6A1E6886-D4BA-48C2-A860-D067AADAC8CE", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Changes Needed:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6A1E6886-D4BA-48C2-A860-D067AADAC8CE", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Changes Needed:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "5336AC78-3B7D-4229-99B0-09E8D43ED058", "D530F451-D1FA-416A-86F1-7E87FBCC4EC7", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Denied:Set Location States:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "5336AC78-3B7D-4229-99B0-09E8D43ED058", "15A2EEA8-53DA-4ADD-9DDE-FF2FCA70023F", @"False" ); // Approval Process:Denied:Set Location States:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5336AC78-3B7D-4229-99B0-09E8D43ED058", "53B06E3D-22CD-488B-86D3-4C4D81511334", @"3" ); // Approval Process:Denied:Set Location States:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "5336AC78-3B7D-4229-99B0-09E8D43ED058", "22FAA8A0-DE01-4402-B0A6-89A5C58B180A", @"False" ); // Approval Process:Denied:Set Location States:Ignore Locations With Approval Groups
            RockMigrationHelper.AddActionTypeAttributeValue( "69F1D448-C818-4A9F-B01D-C136D6D9275A", "4FBD0A05-603C-4F7E-B122-2865F31A4AD0", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Denied:Set Resource States:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "69F1D448-C818-4A9F-B01D-C136D6D9275A", "C04B4BA1-65E6-493F-BD1B-834D6A1961E8", @"False" ); // Approval Process:Denied:Set Resource States:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "69F1D448-C818-4A9F-B01D-C136D6D9275A", "0E07B013-2C74-4CD7-AFBC-C535151AA60A", @"3" ); // Approval Process:Denied:Set Resource States:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "69F1D448-C818-4A9F-B01D-C136D6D9275A", "3CA5B94F-5571-497C-85EF-2586B62BDFC2", @"False" ); // Approval Process:Denied:Set Resource States:Ignore Resources With Approval Groups
            RockMigrationHelper.AddActionTypeAttributeValue( "27DA6AF4-DFAC-4D72-B068-FB5C5B4BD9A4", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Denied:Send Email to Admin Contact:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "27DA6AF4-DFAC-4D72-B068-FB5C5B4BD9A4", "0C4C13B8-7076-4872-925A-F950886B5E16", @"35ae460c-60eb-4dc5-98e9-b77b7c8a181c" ); // Approval Process:Denied:Send Email to Admin Contact:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "27DA6AF4-DFAC-4D72-B068-FB5C5B4BD9A4", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Reservation Denied: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Denied:Send Email to Admin Contact:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "27DA6AF4-DFAC-4D72-B068-FB5C5B4BD9A4", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }} {% assign reservation = Workflow | Attribute:'Reservation','Object' %}  <p> Your reservation has been denied:<br/><br/> Name: {{ reservation.Name }}<br/> Admin Contact: {{ reservation.AdministrativeContactPersonAlias.Person.FullName }}<br/> Event Contact: {{ reservation.EventContactPersonAlias.Person.FullName }}<br/> Campus: {{ reservation.Campus.Name }}<br/> Ministry: {{ reservation.ReservationMinistry.Name }}<br/> Number Attending: {{ reservation.NumberAttending }}<br/> <br/> Schedule: {% execute import:'com.bemaservices.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.FriendlyScheduleText; {% endexecute %}<br/> Event Duration: {% execute import:'com.bemaservices.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.GetCalenderEvent().Duration.Hours + "" hrs "" + reservation.Schedule.GetCalenderEvent().Duration.Minutes + "" min""; {% endexecute %}<br/> Setup Time: {{ reservation.SetupTime }} min<br/> Cleanup Time: {{ reservation.CleanupTime }} min<br/> {% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %} {% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %} <br/> Notes: {{ reservation.Note }}<br/> <br/> <a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a> </p> {{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Denied:Send Email to Admin Contact:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "27DA6AF4-DFAC-4D72-B068-FB5C5B4BD9A4", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Denied:Send Email to Admin Contact:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "79551640-080B-44F9-A00B-1AFB1A6D3AD7", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Denied:Send Email to Event Contact:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "79551640-080B-44F9-A00B-1AFB1A6D3AD7", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Reservation Denied: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Denied:Send Email to Event Contact:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "79551640-080B-44F9-A00B-1AFB1A6D3AD7", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }} {% assign reservation = Workflow | Attribute:'Reservation','Object' %}  <p> Your reservation has been denied:<br/><br/> Name: {{ reservation.Name }}<br/> Admin Contact: {{ reservation.AdministrativeContactPersonAlias.Person.FullName }}<br/> Event Contact: {{ reservation.EventContactPersonAlias.Person.FullName }}<br/> Campus: {{ reservation.Campus.Name }}<br/> Ministry: {{ reservation.ReservationMinistry.Name }}<br/> Number Attending: {{ reservation.NumberAttending }}<br/> <br/> Schedule: {% execute import:'com.bemaservices.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.FriendlyScheduleText; {% endexecute %}<br/> Event Duration: {% execute import:'com.bemaservices.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.GetCalenderEvent().Duration.Hours + "" hrs "" + reservation.Schedule.GetCalenderEvent().Duration.Minutes + "" min""; {% endexecute %}<br/> Setup Time: {{ reservation.SetupTime }} min<br/> Cleanup Time: {{ reservation.CleanupTime }} min<br/> {% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %} {% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %} <br/> Notes: {{ reservation.Note }}<br/> <br/> <a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a> </p> {{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Denied:Send Email to Event Contact:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "79551640-080B-44F9-A00B-1AFB1A6D3AD7", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Denied:Send Email to Event Contact:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "22043E5A-45F7-41C4-93AF-9308B1222291", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Denied:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "22043E5A-45F7-41C4-93AF-9308B1222291", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Denied:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "D4B2E0BB-6FC7-4B68-AF08-72B4A8CD7C7E", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Cancelled:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D4B2E0BB-6FC7-4B68-AF08-72B4A8CD7C7E", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Cancelled:Complete Workflow:Status|Status Attribute

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"
			UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )
			FROM [AttributeQualifier] [aq]
			INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]
			INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
			INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]
			WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'
			AND [aq].[key] = 'definedtypeguid'
		" );

            #endregion
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}