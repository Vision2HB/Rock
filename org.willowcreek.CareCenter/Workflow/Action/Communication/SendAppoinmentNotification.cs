using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using org.willowcreek.CareCenter.Model;


using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace org.willowcreek.CareCenter.Workflow.Action
{
    /// <summary>
    /// Sends an appointment notification.
    /// </summary>
    [ActionCategory( "Communications" )]
    [Description( "Sends an appointment notification." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Send Appointment Notification" )]

    [WorkflowTextOrAttribute( "Send To Email Addresses", "Attribute Value", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", true, "", "", 1, "To",
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    public class SendAppointmentNotification : ActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            if ( action.Activity != null )
            {
                var notification = new WorkflowAppointmentService( rockContext )
                    .Queryable()
                    .Where( w =>
                        w.WorkflowId == action.Activity.WorkflowId &&
                        w.TimeSlot != null &&
                        w.TimeSlot.Notification != null )
                    .Select( w => w.TimeSlot.Notification )
                    .FirstOrDefault();

                if ( notification != null )
                {
                    Guid? personAliasGuid = GetAttributeValue( action, "To", true ).AsGuidOrNull();
                    if ( personAliasGuid.HasValue )
                    {
                        var person = new PersonAliasService( rockContext ).Queryable()
                            .Where( a => a.Guid.Equals( personAliasGuid.Value ) )
                            .Select( a => a.Person )
                            .FirstOrDefault();
                        if ( person != null && !string.IsNullOrWhiteSpace( person.Email ) )
                        {
                            var recipientList = new List<string> { person.Email };
                            string from = string.Format( "{0} <{1}>", notification.FromName, notification.FromEmail );
                            string subject = notification.AnnouncementSubject;
                            string body = notification.AnnouncementMessage;

                            var mergeFields = GetMergeFields( action );

                            var mediumData = new Dictionary<string, string>();
                            mediumData.Add( "From", from.ResolveMergeFields( mergeFields ) );
                            mediumData.Add( "Subject", subject.ResolveMergeFields( mergeFields ) );
                            mediumData.Add( "Body", System.Text.RegularExpressions.Regex.Replace( body.ResolveMergeFields( mergeFields ), @"\[\[\s*UnsubscribeOption\s*\]\]", string.Empty ) );

                            var mediumEntity = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid(), rockContext );
                            if ( mediumEntity != null )
                            {
                                var medium = MediumContainer.GetComponent( mediumEntity.Name );
                                if ( medium != null && medium.IsActive )
                                {
                                    var transport = medium.Transport;
                                    if ( transport != null && transport.IsActive )
                                    {
                                        var appRoot = GlobalAttributesCache.Read( rockContext ).GetValue( "InternalApplicationRoot" );

                                        if ( transport is Rock.Communication.Transport.SMTPComponent )
                                        {
                                            ( (Rock.Communication.Transport.SMTPComponent)transport ).Send( mediumData, recipientList, appRoot, string.Empty, false );
                                        }
                                        else
                                        {
                                            transport.Send( mediumData, recipientList, appRoot, string.Empty );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;

        }
    }
}
