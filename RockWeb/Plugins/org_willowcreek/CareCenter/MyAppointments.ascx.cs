using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using org.willowcreek.CareCenter.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Care Center block for viewing list of appointments for a specific persons family.
    /// </summary>
    [DisplayName( "My Appointments" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for viewing list of appointments for a specific persons family." )]

    [IntegerField("Max Items", "The maximum number of appointments to show in the list", false, 10, "", 0)]
    [LinkedPage("Appointment List Page", "Page for displaying all of the appointments", true, "", "", 1)]
    [LinkedPage( "Appointment Detail Page", "Page for displaying details of a specific appointment", true, "", "", 2 )]
    public partial class MyAppointments : PersonBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rptAppointments.ItemCommand += RptAppointments_ItemCommand;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                GetData();
            }
        }

        #endregion

        #region Events

        private void RptAppointments_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var parms = new Dictionary<string, string> { { "AppointmentId", e.CommandArgument.ToString() } };
            NavigateToLinkedPage( "AppointmentDetailPage", parms );
        }

        protected void lbShowAll_Click( object sender, EventArgs e )
        {
            if ( Person != null )
            {
                var parms = new Dictionary<string, string> { { "PersonGuid", Person.Guid.ToString() } };
                NavigateToLinkedPage( "AppointmentListPage", parms );
            }
        }

        #endregion

        #region Methods

        private void GetData()
        {
            using ( var rockContext = new RockContext() )
            {
                var appointmentQry = new WorkflowAppointmentService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a => a.PersonAlias != null );

                if ( Person != null )
                { 
                    var today = RockDateTime.Today;
                    appointmentQry = appointmentQry.Where( v =>
                        v.PersonAlias.PersonId == Person.Id &&
                        v.AppointmentDate >= today );

                    int take = GetAttributeValue( "MaxItems" ).AsIntegerOrNull() ?? 10;

                    rptAppointments.DataSource = appointmentQry
                        .OrderByDescending( v => v.AppointmentDate )
                        .Take( take )
                        .ToList()
                        .Select( v => new
                        {
                            v.Id,
                            v.AppointmentDate,
                            Person = v.PersonAlias != null && v.PersonAlias.Person != null ? v.PersonAlias.Person.FullName : "",
                            Workflow = v.Workflow != null && v.Workflow.WorkflowType != null ? v.Workflow.WorkflowType.Name : ""
                        } );
                    rptAppointments.DataBind();
                }
            }
        }

        #endregion



    }
}