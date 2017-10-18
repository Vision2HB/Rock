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
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Care Center block for printing appointment details.
    /// </summary>
    [DisplayName( "Appointment Lava" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for printing appointment details." )]

    [CodeEditorField( "Lava Template", "The Lava template to use for the appointment.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 500, true, @"
<style>
    .food-box {
        float: right;
        width: 200px;
        height: 40px;
        margin: 5px;
        border: 1px solid rgba(0, 0, 0, .2);
    }
</style>

<div class='clearfix'>
    <div class='pull-right'>
        <a href='#' class='btn btn-primary hidden-print' onClick='window.print();'><i class='fa fa-print'></i> Print</a> 
    </div>
</div>

<div>
    <div class='row margin-b-xl'>
        <div class='col-md-6'>
            <div class='pull-left'>
                <img src='/Themes/CareCenter/Assets/Images/logo.png' width='300px' />
            </div>
        </div>
        <div class='col-md-6 text-right'>
        </div>
    </div>

    <h3>Guest Appointment</h3>
    <hr>
    {{ Appointment.PersonAlias.Person.FullName }},<br/><br/>
    You have a <strong>{{ Appointment.TimeSlot.ServiceArea.Name }}</strong> appointment scheduled with
    the Willow Creek Care Center on <strong>{{ Appointment.AppointmentDate| Date:'dddd, MMMM d, yyyy' }}</strong>
    at <strong>{{ Appointment.TimeSlot.ScheduleTitle | Date:'hh:mm tt' }}</strong>.<br/>
</div>
", "", 0 )]
    [BooleanField( "Enable Debug", "Shows the merge fields available for the Lava", order: 1 )]
    public partial class AppointmentLava : RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                DisplayResults();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            DisplayResults();
        }

        #endregion

        #region Methods

        private void DisplayResults()
        {
            using ( var rockContext = new RockContext() )
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

                var apptId = PageParameter( "AppointmentId" ).AsIntegerOrNull();
                if ( apptId.HasValue )
                {
                    var appt = new WorkflowAppointmentService( rockContext ).Queryable().AsNoTracking()
                        .Where( a => a.Id == apptId.Value )
                        .FirstOrDefault();
                    if ( appt != null )
                    {
                        mergeFields.Add( "Appointment", appt);
                    }
                }

                var template = GetAttributeValue( "LavaTemplate" );
                lResults.Text = template.ResolveMergeFields( mergeFields );

                // show debug info
                if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && UserCanEdit )
                {
                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo();
                }
            }
        }

        #endregion



    }
}