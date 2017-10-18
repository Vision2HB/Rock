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
    /// Care Center block for viewing list of visits.
    /// </summary>
    [DisplayName( "Visit List" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for viewing list of visits." )]

    [LinkedPage("Detail Page", "Page for displaying details of a visit", true, "", "", 0)]
    [LinkedPage( "Passport Page", "The Page for viewing and printing passports", true, "", "", 1 )]
    [LinkedPage( "Person Profile Page", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", false, "", "", 2, "PersonProfilePage" )]
    [WorkflowTypeField("Passport Workflows", "The workflow types that need a passport", true, false, "", "", 3)]
    public partial class VisitList : Rock.Web.UI.RockBlock
    {
        #region Fields

        private HyperLink hlPrintPassport;
        private List<int> _visitsWithNotes;
        private List<Guid> _passportWorkflows;
        private Person _person;

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

            gfVisitFilter.ApplyFilterClick += gfVisitFilter_ApplyFilterClick;
            gfVisitFilter.ClearFilterClick += gfVisitFilter_ClearFilterClick;
            gfVisitFilter.DisplayFilterValue += gfVisitFilter_DisplayFilterValue;
            gfVisitFilter.AdditionalFilterDisplay.Add( "Visit Date", "Current Day" );

            gVisits.DataKeyNames = new string[] { "Id" };
            gVisits.Actions.ShowAdd = false;
            gVisits.RowDataBound += GVisits_RowDataBound;
            gVisits.GridRebind += gVisits_GridRebind;
            gVisits.IsDeleteEnabled = UserCanEdit;
            gVisits.ShowConfirmDeleteDialog = false;

            hlPrintPassport = new HyperLink();
            hlPrintPassport.ID = "lbPrintPassport";
            hlPrintPassport.CssClass = "btn btn-primary btn-sm";
            hlPrintPassport.Text = "Print Passports";
            hlPrintPassport.Target = "_blank";

            gVisits.Actions.AddCustomActionControl( hlPrintPassport );

            _passportWorkflows = GetAttributeValue( "PassportWorkflows" ).SplitDelimitedValues().AsGuidList();

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

            nbResult.Visible = false;

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            string url = LinkedPageUrl( "PassportPage", new Dictionary<string, string> { { "VisitIds", "9999" } } );
            string script = string.Format( @"
    $('#{0}').click( function() {{
        var selected = [];
        $('#{1}').find('input[id$=""_cbSelect_0""]:checked').each( function() {{
            selected.push($(this).closest('tr').attr('datakey'));
        }});
        var urlFormat = '{2}';
        var url = urlFormat.replace('9999',selected.join());
        window.open( url, 'passport' ); 
        return false;
    }});
", hlPrintPassport.ClientID, gVisits.ClientID, url );
            ScriptManager.RegisterStartupScript( hlPrintPassport, hlPrintPassport.GetType(), "printPassport", script, true );
        }

        #endregion

        #region Events

        /// <summary>
        /// Gfs the visit filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void gfVisitFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Pager Number":
                case "First Name":
                case "Last Name":
                    {
                        break;
                    }

                case "Visit Date":
                    {
                        e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }

                case "Status":
                    {
                        var status = e.Value.ConvertToEnumOrNull<VisitStatus>();
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
        /// Handles the ClearFilterClick event of the gfVisitFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gfVisitFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfVisitFilter.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfVisitFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gfVisitFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfVisitFilter.SaveUserPreference( "Pager Number", nbPagerNumber.Text );
            gfVisitFilter.SaveUserPreference( "Visit Date", drpVisitDate.DelimitedValues );
            gfVisitFilter.SaveUserPreference( "First Name", tbFirstName.Text );
            gfVisitFilter.SaveUserPreference( "Last Name", tbLastName.Text );
            gfVisitFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the GVisits control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void GVisits_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var visit = e.Row.DataItem as Visit;
                if ( visit != null )
                {
                    // Profile Link
                    var aProfileLink = e.Row.FindControl( "aProfileLink" ) as HtmlAnchor;
                    if ( aProfileLink != null )
                    {
                        if ( visit.PersonAlias != null && visit.PersonAlias.Person != null )
                        {
                            var qryParams = new Dictionary<string, string> { { "PersonId", visit.PersonAlias.Person.Id.ToString() } };
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
                        switch( visit.Status )
                        {
                            case VisitStatus.Active: labelClass = "success"; break;
                            case VisitStatus.Cancelled: labelClass = "danger"; break;
                            default: labelClass = "default"; break;
                        }
                        lStatus.Text = string.Format( "<span class='label label-{0}'>{1}</span>", labelClass, visit.Status.ConvertToString() );
                    }

                    bool passportReady = false;

                    // Service Areas
                    var lServiceAreas = e.Row.FindControl( "lServiceAreas" ) as Literal;
                    if ( lServiceAreas != null )
                    {
                        var serviceAreas = new StringBuilder();
                        foreach ( var workflow in visit.Workflows.OrderBy( w => w.WorkflowTypeCache.Order ) )
                        {
                            string labelClass = "default";
                            switch ( workflow.Status )
                            {
                                case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_PENDING: labelClass = "default"; break;
                                case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_WAITING:
                                    {
                                        labelClass = "success";
                                        if ( _passportWorkflows.Contains( workflow.WorkflowTypeCache.Guid ) && visit.PassportStatus != PassportStatus.Printed )
                                        {
                                            passportReady = true;
                                        }
                                        break;
                                    }
                                case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_INPROGRESS: labelClass = "success"; break;
                                case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_COMPLETED: labelClass = "default"; break;
                                case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_CANCELLED: labelClass = "danger"; break;
                                default: labelClass = "default"; break;
                            }
                            string label = string.Format( "<span class='label label-{0}'>{1}</span>", labelClass, workflow.Status );
                            serviceAreas.AppendFormat( "<div class=''>{0} {1}</div>", workflow.WorkflowTypeCache.Name, label );
                        }
                        lServiceAreas.Text = serviceAreas.ToString();
                    }

                    if ( !passportReady )
                    {
                        var selectCheckBox = e.Row.FindControl( "cbSelect_0" ) as CheckBox;
                        if ( selectCheckBox != null )
                        {
                            selectCheckBox.Visible = false;
                        }
                    }

                    // First Visit
                    var lFirstVisit = e.Row.FindControl( "lFirstVisit" ) as Literal;
                    if ( lFirstVisit != null && visit.PersonAlias != null )
                    {
                        if ( visit.IsFirstVisit )
                        {
                            lFirstVisit.Text = "<i class='fa fa-check'></i>";
                        }
                    }

                    // Passport 
                    var lPassport = e.Row.FindControl( "lPassport" ) as Literal;
                    if ( lPassport != null )
                    {
                        if ( visit.PassportStatus != PassportStatus.NotNeeded )
                        {
                            string faIcon = visit.PassportStatus == PassportStatus.Printed ? "fa fa-file-text-o text-muted" : "fa fa-print";
                            lPassport.Text = string.Format( "<i class='{0} fa-2x'></i>", faIcon );
                        }
                    }

                    // Notes
                    if ( _visitsWithNotes != null && _visitsWithNotes.Any( v => v == visit.Id ) )
                    {
                        var lNotes = e.Row.FindControl( "lNotes" ) as Literal;
                        if ( lNotes != null )
                        {
                            lNotes.Text = "<i class='fa fa-comment fa-2x'></i>";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gVisits control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gVisits_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlFilter_SelectedIndexChanged( object sender, EventArgs e )
        {
            gfVisitFilter.SaveUserPreference( "Service Area", ddlServiceArea.SelectedValue );
            gfVisitFilter.SaveUserPreference( "Passport Status", ddlPassportStatus.SelectedValue );
            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gVisits control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gVisits_RowSelected( object sender, RowEventArgs e )
        {
            var qryParams = new Dictionary<string, string> { { "VisitId", e.RowKeyId.ToString() } };
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
                gfVisitFilter.Visible = false;
            }
            else
            {
                nbPagerNumber.Text = gfVisitFilter.GetUserPreference( "Pager Number" );
                drpVisitDate.DelimitedValues = gfVisitFilter.GetUserPreference( "Visit Date" );
                tbFirstName.Text = gfVisitFilter.GetUserPreference( "First Name" );
                tbLastName.Text = gfVisitFilter.GetUserPreference( "Last Name" );

                ddlStatus.SelectedIndex = -1;
                ddlStatus.Items.Clear();
                ddlStatus.BindToEnum<VisitStatus>( true );
                ddlStatus.SetValue( gfVisitFilter.GetUserPreference( "Status" ) );

                ddlServiceArea.SelectedIndex = -1;
                ddlServiceArea.Items.Clear();
                ddlServiceArea.Items.Add( new ListItem() );
                using ( var rockContext = new RockContext() )
                {
                    foreach ( var serviceArea in new ServiceAreaService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a => a.WorkflowTypeId.HasValue )
                        .OrderBy( a => a.Order )
                        .ToList() )
                    {
                        ddlServiceArea.Items.Add( new ListItem( serviceArea.Name, serviceArea.Id.ToString() ) );
                    }
                }
                ddlServiceArea.SetValue( gfVisitFilter.GetUserPreference( "Service Area" ) );

                ddlPassportStatus.BindToEnum<PassportStatus>( true );
                ddlPassportStatus.SetValue( gfVisitFilter.GetUserPreference( "Passport Status" ) );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var visitQry = new VisitService( rockContext )
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

                    visitQry = visitQry.Where( v => personIds.Contains( v.PersonAlias.PersonId ) );
                }
                else
                {
                    var dateTimeField = gVisits.ColumnsOfType<RockBoundField>().Where( c => c.HeaderText == "Intake Date/Time" ).First();
                    var timeField = gVisits.ColumnsOfType<RockBoundField>().Where( c => c.HeaderText == "Intake Time" ).First();

                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( gfVisitFilter.GetUserPreference( "Visit Date" ) );
                    if ( dateRange.Start.HasValue || dateRange.End.HasValue )
                    {
                        dateTimeField.Visible = true;
                        timeField.Visible = false;

                        if ( dateRange.Start.HasValue )
                        {
                            visitQry = visitQry
                                .Where( v => v.VisitDate >= dateRange.Start.Value );
                        }
                        if ( dateRange.End.HasValue )
                        {
                            visitQry = visitQry
                                .Where( v => v.VisitDate < dateRange.End.Value );
                        }
                    }
                    else
                    {
                        dateTimeField.Visible = false;
                        timeField.Visible = true;

                        var today = RockDateTime.Today;
                        var tomorrow = today.AddDays( 1 );
                        visitQry = visitQry
                            .Where( v =>
                                v.VisitDate >= today &&
                                v.VisitDate < tomorrow );
                    }

                    var visitStatus = gfVisitFilter.GetUserPreference( "Status" ).ConvertToEnumOrNull<VisitStatus>();
                    if ( visitStatus.HasValue )
                    {
                        visitQry = visitQry
                            .Where( v => v.Status == visitStatus.Value );
                    }

                    int? pagerNumber = gfVisitFilter.GetUserPreference( "Pager Number" ).AsIntegerOrNull();
                    if ( pagerNumber.HasValue )
                    {
                        visitQry = visitQry
                            .Where( v =>
                                v.PagerId.HasValue &&
                                v.PagerId.Value == pagerNumber.Value );
                    }

                    string firstName = gfVisitFilter.GetUserPreference( "First Name" );
                    if ( !string.IsNullOrWhiteSpace( firstName ) )
                    {
                        visitQry = visitQry
                            .Where( v =>
                                v.PersonAlias.Person.NickName.StartsWith( firstName ) ||
                                v.PersonAlias.Person.FirstName.StartsWith( firstName ) );
                    }

                    string lastName = gfVisitFilter.GetUserPreference( "Last Name" );
                    if ( !string.IsNullOrWhiteSpace( lastName ) )
                    {
                        visitQry = visitQry
                            .Where( v => v.PersonAlias.Person.LastName.StartsWith( lastName ) );
                    }

                    int? serviceArea = gfVisitFilter.GetUserPreference( "Service Area" ).AsIntegerOrNull();
                    if ( serviceArea.HasValue )
                    {
                        var workflowTypeIds = new ServiceAreaService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( s => s.Id == serviceArea.Value )
                            .Select( s => s.WorkflowTypeId )
                            .ToList();

                        visitQry = visitQry
                            .Where( v => v.Workflows.Any( w => workflowTypeIds.Contains( w.WorkflowTypeId ) ) );
                    }

                    var passportStatus = gfVisitFilter.GetUserPreference( "Passport Status" ).ConvertToEnumOrNull<PassportStatus>();
                    if ( passportStatus.HasValue )
                    {
                        visitQry = visitQry
                            .Where( v => v.PassportStatus == passportStatus.Value );
                    }
                }

                var sortProperty = gVisits.SortProperty;
                if ( sortProperty != null )
                {
                    gVisits.SetLinqDataSource<Visit>( visitQry.Sort( sortProperty ) );
                }
                else
                {
                    gVisits.SetLinqDataSource<Visit>( visitQry.OrderBy( v => v.VisitDate ) );
                }

                _visitsWithNotes = new List<int>();
                var currentVisits = gVisits.DataSource as List<Visit>;
                if ( currentVisits != null )
                {
                    var visitIds = currentVisits.Select( v => v.Id ).ToList();
                    
                    // Get visits with any notes
                    var visitEntityType = EntityTypeCache.Read( typeof(Visit) );
                    if ( visitEntityType != null )
                    {
                        _visitsWithNotes = new NoteService( rockContext ).Queryable().AsNoTracking()
                            .Where( n =>
                                n.NoteType.EntityTypeId == visitEntityType.Id &&
                                n.EntityId.HasValue &&
                                visitIds.Contains( n.EntityId.Value ) )
                            .Select( n => n.EntityId.Value )
                            .ToList()
                            .Distinct()
                            .ToList();
                    }
                }

                gVisits.DataBind();

            }
        }

        #endregion

    }
}