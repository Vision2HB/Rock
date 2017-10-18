using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using org.willowcreek.CareCenter.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Care Center block for viewing list of apointments.
    /// </summary>
    [DisplayName( "Appointment List" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for viewing list of appointments." )]

    [LinkedPage("Detail Page", "Page for displaying details of a appointment", true, "", "", 0)]
    [LinkedPage( "Person Profile Page", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", false, "", "", 1, "PersonProfilePage" )]
    public partial class AppointmentList : Rock.Web.UI.RockBlock
    {
        #region Fields

        private LinkButton lbPrintPassport;
        private List<int> _appointmentsWithNotes;
        private Person _person;
        private bool _isExporting = false;

        #endregion

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

            gfAppointmentFilter.ApplyFilterClick += gfAppointmentFilter_ApplyFilterClick;
            gfAppointmentFilter.ClearFilterClick += gfAppointmentFilter_ClearFilterClick;
            gfAppointmentFilter.DisplayFilterValue += gfAppointmentFilter_DisplayFilterValue;
            gfAppointmentFilter.AdditionalFilterDisplay.Add( "Appointment Date", "Current Day" );

            gAppointments.DataKeyNames = new string[] { "Id" };
            gAppointments.Actions.ShowAdd = false;
            gAppointments.RowDataBound += GAppointments_RowDataBound;
            gAppointments.GridRebind += gAppointments_GridRebind;
            gAppointments.IsDeleteEnabled = UserCanEdit;

            Guid? personGuid = PageParameter( "PersonGuid" ).AsGuidOrNull();
            if ( personGuid.HasValue )
            {
                _person = new PersonService( new RockContext() ).Get( personGuid.Value );
            }

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
                BindFilter();
                BindGrid( false );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Gfs the appointment filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void gfAppointmentFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "First Name":
                case "Last Name":
                    {
                        break;
                    }

                case "Appointment Date":
                    {
                        e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }

                case "Status":
                    {
                        var status = e.Value.ConvertToEnumOrNull<AppointmentStatus>();
                        if ( status.HasValue )
                        {
                            e.Value = status.ConvertToString();
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfAppointmentFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gfAppointmentFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfAppointmentFilter.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfAppointmentFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gfAppointmentFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfAppointmentFilter.SaveUserPreference( "Appointment Date", drpAppointmentDate.DelimitedValues );
            gfAppointmentFilter.SaveUserPreference( "First Name", tbFirstName.Text );
            gfAppointmentFilter.SaveUserPreference( "Last Name", tbLastName.Text );
            gfAppointmentFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );

            BindGrid( false );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid( false );
        }

        private void GAppointments_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var appointment = e.Row.DataItem as WorkflowAppointment;
                if ( appointment != null )
                {
                    // Profile Link
                    var aProfileLink = e.Row.FindControl( "aProfileLink" ) as HtmlAnchor;
                    if ( aProfileLink != null )
                    {
                        if ( appointment.PersonAlias != null  )
                        {
                            var qryParams = new Dictionary<string, string> { { "PersonId", appointment.PersonAlias.PersonId.ToString() } };
                            aProfileLink.HRef = LinkedPageUrl( "PersonProfilePage", qryParams );
                            aProfileLink.Visible = true;
                        }
                        else
                        {
                            aProfileLink.Visible = false;
                        }
                    }

                    // Status
                    var lStatus = e.Row.FindControl( "lStatus" ) as Literal;
                    if ( lStatus != null )
                    {
                        string labelClass = "default";
                        switch( appointment.Status )
                        {
                            case AppointmentStatus.Active: labelClass = "success"; break;
                            case AppointmentStatus.NoShow: labelClass = "danger"; break;
                            case AppointmentStatus.Cancelled: labelClass = "warning"; break;
                            default: labelClass = "default"; break;
                        }
                        lStatus.Text = _isExporting ? appointment.Status.ConvertToString() : string.Format( "<span class='label label-{0}'>{1}</span>", labelClass, appointment.Status.ConvertToString() );
                    }

                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gAppointments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAppointments_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid( e.IsExporting );
        }

        protected void ddlServiceAreas_SelectedIndexChanged( object sender, EventArgs e )
        {
            gfAppointmentFilter.SaveUserPreference( "Service Area", ddlServiceAreas.SelectedValue );
            BindGrid( false );
        }

        protected void gAppointments_RowSelected( object sender, RowEventArgs e )
        {
            var qryParams = new Dictionary<string, string> { { "AppointmentId", e.RowKeyId.ToString() } };
            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        #endregion

        #region Methods

        private void BindFilter()
        {
            if ( _person != null )
            {
                lPersonName.Text = _person.FullName;
                pnlMainFilter.Visible = false;
                gfAppointmentFilter.Visible = false;
            }
            else
            {
                drpAppointmentDate.DelimitedValues = gfAppointmentFilter.GetUserPreference( "Appointment Date" );
                tbFirstName.Text = gfAppointmentFilter.GetUserPreference( "First Name" );
                tbLastName.Text = gfAppointmentFilter.GetUserPreference( "Last Name" );

                ddlStatus.SelectedIndex = -1;
                ddlStatus.Items.Clear();
                ddlStatus.BindToEnum<AppointmentStatus>( true );
                ddlStatus.SetValue( gfAppointmentFilter.GetUserPreference( "Status" ) );

                ddlServiceAreas.SelectedIndex = -1;
                ddlServiceAreas.Items.Clear();
                ddlServiceAreas.Items.Add( new ListItem() );
                using ( var rockContext = new RockContext() )
                {
                    foreach ( var serviceArea in new ServiceAreaService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a => a.AppointmentTimeSlots
                            .Where( t =>
                                t.IsActive == true &&
                                t.Schedule != null &&
                                t.ServiceArea != null &&
                                t.ServiceArea.WorkflowType != null )
                            .Any() )
                        .OrderBy( a => a.Name ) )
                    {
                        ddlServiceAreas.Items.Add( new ListItem( serviceArea.Name, serviceArea.Id.ToString() ) );
                    }
                }
                ddlServiceAreas.SetValue( gfAppointmentFilter.GetUserPreference( "Service Area" ) );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid( bool isExporting )
        {
            _isExporting = isExporting;
            if ( _isExporting )
            {
                gAppointments.ColumnsOfType<RockTemplateFieldUnselected>().First().Visible = false;
            }

            using ( var rockContext = new RockContext() )
            {
                var appointmentQry = new WorkflowAppointmentService( rockContext )
                    .Queryable().AsNoTracking();

                if ( _person != null )
                {
                    var personIds = new List<int>();
                    foreach ( var family in _person.GetFamilies() )
                    {
                        foreach ( var personId in family.Members.Select( m => m.PersonId ).ToList() )
                        {
                            personIds.Add( personId );
                        }
                    }

                    appointmentQry = appointmentQry
                        .Where( a => personIds.Contains( a.PersonAlias.PersonId ) );
                }
                else
                {
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( gfAppointmentFilter.GetUserPreference( "Appointment Date" ) );
                    if ( dateRange.Start.HasValue || dateRange.End.HasValue )
                    {
                        if ( dateRange.Start.HasValue )
                        {
                            appointmentQry = appointmentQry
                                .Where( v => v.AppointmentDate >= dateRange.Start.Value );
                        }
                        if ( dateRange.End.HasValue )
                        {
                            appointmentQry = appointmentQry
                                .Where( v => v.AppointmentDate < dateRange.End.Value );
                        }
                    }
                    else
                    {
                        var today = RockDateTime.Today;
                        var tomorrow = today.AddDays( 1 );
                        appointmentQry = appointmentQry
                            .Where( v =>
                                v.AppointmentDate >= today &&
                                v.AppointmentDate < tomorrow );
                    }

                    var appointmentStatus = gfAppointmentFilter.GetUserPreference( "Status" ).ConvertToEnumOrNull<AppointmentStatus>();
                    if ( appointmentStatus.HasValue )
                    {
                        appointmentQry = appointmentQry
                            .Where( v => v.Status == appointmentStatus.Value );
                    }

                    string firstName = gfAppointmentFilter.GetUserPreference( "First Name" );
                    if ( !string.IsNullOrWhiteSpace( firstName ) )
                    {
                        appointmentQry = appointmentQry
                            .Where( v =>
                                v.PersonAlias.Person.NickName.StartsWith( firstName ) ||
                                v.PersonAlias.Person.FirstName.StartsWith( firstName ) );
                    }

                    string lastName = gfAppointmentFilter.GetUserPreference( "Last Name" );
                    if ( !string.IsNullOrWhiteSpace( lastName ) )
                    {
                        appointmentQry = appointmentQry
                            .Where( v => v.PersonAlias.Person.LastName.StartsWith( lastName ) );
                    }

                    int? serviceArea = gfAppointmentFilter.GetUserPreference( "Service Area" ).AsIntegerOrNull();
                    if ( serviceArea.HasValue )
                    {
                        appointmentQry = appointmentQry
                            .Where( v => v.TimeSlot != null && v.TimeSlot.ServiceAreaId == serviceArea.Value );
                    }
                }

                var sortProperty = gAppointments.SortProperty;
                if ( sortProperty != null )
                {
                    gAppointments.SetLinqDataSource<WorkflowAppointment>( appointmentQry.Sort( sortProperty ) );
                }
                else
                {
                    var appointmentList = appointmentQry.ToList();
                    gAppointments.DataSource = appointmentList.OrderBy( v => v.AppointmentDate ).ThenBy( v => v.TimeSlot.Schedule.StartTimeOfDay ).ToList();
                }

                gAppointments.DataBind();

            }
        }

        #endregion

    }
}