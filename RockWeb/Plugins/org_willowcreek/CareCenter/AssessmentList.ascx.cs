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
    /// Care Center block for viewing list of assessments.
    /// </summary>
    [DisplayName( "Assessment List" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for viewing list of assessments." )]

    [LinkedPage("Detail Page", "Page for displaying details of an assessment", true, "", "", 0)]
    [LinkedPage( "Person Profile Page", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", false, "", "", 2, "PersonProfilePage" )]
    public partial class AssessmentList : Rock.Web.UI.RockBlock
    {
        #region Fields

        private List<int> _assessmentsWithNotes;
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

            gfAssessmentFilter.ApplyFilterClick += gfAssessmentFilter_ApplyFilterClick;
            gfAssessmentFilter.ClearFilterClick += gfAssessmentFilter_ClearFilterClick;
            gfAssessmentFilter.DisplayFilterValue += gfAssessmentFilter_DisplayFilterValue;
            gfAssessmentFilter.AdditionalFilterDisplay.Add( "Assessment Date", "Current Day" );

            gAssessments.DataKeyNames = new string[] { "Id" };
            gAssessments.Actions.ShowAdd = false;
            gAssessments.RowDataBound += GAssessments_RowDataBound;
            gAssessments.GridRebind += gAssessments_GridRebind;
            gAssessments.IsDeleteEnabled = UserCanEdit;
            gAssessments.ShowConfirmDeleteDialog = false;

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
        }

        #endregion

        #region Events

        /// <summary>
        /// Gfs the assessment filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void gfAssessmentFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "First Name":
                case "Last Name":
                    {
                        break;
                    }

                case "Assessment Date":
                    {
                        e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }

                case "Include Completed":
                    {
                        e.Value = e.Value.ToString().AsBoolean() ? "Yes" : string.Empty;
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
        /// Handles the ClearFilterClick event of the gfAssessmentFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gfAssessmentFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfAssessmentFilter.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfAssessmentFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gfAssessmentFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfAssessmentFilter.SaveUserPreference( "Assessment Date", drpAssessmentDate.DelimitedValues );
            gfAssessmentFilter.SaveUserPreference( "First Name", tbFirstName.Text );
            gfAssessmentFilter.SaveUserPreference( "Last Name", tbLastName.Text );
            gfAssessmentFilter.SaveUserPreference( "Include Completed", cbIncludeCompleted.Checked.ToString() );

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
        /// Handles the RowDataBound event of the GAssessments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void GAssessments_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var assessment = e.Row.DataItem as Assessment;
                if ( assessment != null )
                {
                    // Profile Link
                    var aProfileLink = e.Row.FindControl( "aProfileLink" ) as HtmlAnchor;
                    if ( aProfileLink != null )
                    {
                        if ( assessment.PersonAlias != null && assessment.PersonAlias.Person != null )
                        {
                            var qryParams = new Dictionary<string, string> { { "PersonId", assessment.PersonAlias.Person.Id.ToString() } };
                            aProfileLink.HRef = LinkedPageUrl( "PersonProfilePage", qryParams );
                            aProfileLink.Visible = true;
                        }
                        else
                        {
                            aProfileLink.Visible = false;
                        }
                    }

                    // Assessment Types
                    var lAssessmentTypes = e.Row.FindControl( "lAssessmentTypes" ) as Literal;
                    if ( lAssessmentTypes != null )
                    {
                        var assessmentTypes = new StringBuilder();
                        foreach ( var workflow in assessment.Workflows.OrderBy( w => w.WorkflowType.Order ) )
                        {
                            string labelClass = "default";
                            switch ( workflow.Status )
                            {
                                case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_PENDING: labelClass = "default"; break;
                                case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_WAITING: labelClass = "success"; break;
                                case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_INPROGRESS: labelClass = "success"; break;
                                case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_COMPLETED: labelClass = "default"; break;
                                case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_CANCELLED: labelClass = "danger"; break;
                                default: labelClass = "default"; break;
                            }
                            string label = string.Format( "<span class='label label-{0}'>{1}</span>", labelClass, workflow.Status );
                            assessmentTypes.AppendFormat( "<div class=''>{0} {1}</div>", workflow.WorkflowType.Name, label );
                        }
                        lAssessmentTypes.Text = assessmentTypes.ToString();
                    }

                    // First Assessment
                    var lFirstAssessment = e.Row.FindControl( "lFirstAssessment" ) as Literal;
                    if ( lFirstAssessment != null && assessment.PersonAlias != null )
                    {
                        if ( assessment.IsFirstAssessment )
                        {
                            lFirstAssessment.Text = "<i class='fa fa-check'></i>";
                        }
                    }

                    // Notes
                    if ( _assessmentsWithNotes != null && _assessmentsWithNotes.Any( v => v == assessment.Id ) )
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
        /// Handles the GridRebind event of the gAssessments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAssessments_GridRebind( object sender, EventArgs e )
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
            gfAssessmentFilter.SaveUserPreference( "Assessment Type", ddlAssessmentType.SelectedValue );
            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gAssessments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAssessments_RowSelected( object sender, RowEventArgs e )
        {
            var qryParams = new Dictionary<string, string> { { "AssessmentId", e.RowKeyId.ToString() } };
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
                gfAssessmentFilter.Visible = false;
            }
            else
            {
                drpAssessmentDate.DelimitedValues = gfAssessmentFilter.GetUserPreference( "Assessment Date" );
                tbFirstName.Text = gfAssessmentFilter.GetUserPreference( "First Name" );
                tbLastName.Text = gfAssessmentFilter.GetUserPreference( "Last Name" );
                cbIncludeCompleted.Checked = gfAssessmentFilter.GetUserPreference( "Include Completed" ).AsBoolean();

                ddlAssessmentType.SelectedIndex = -1;
                ddlAssessmentType.Items.Clear();
                ddlAssessmentType.Items.Add( new ListItem() );
                using ( var rockContext = new RockContext() )
                {
                    var assessmentWorkflowTypeIds = new AssessmentService( rockContext )
                        .Queryable().AsNoTracking()
                        .SelectMany( a => a.Workflows.Select( w => w.WorkflowTypeId ) );

                    foreach( var workflowType in new WorkflowTypeService( rockContext ) 
                        .Queryable().AsNoTracking()
                        .Where( w => assessmentWorkflowTypeIds.Contains( w.Id ) )
                        .OrderBy( w => w.Order )
                        .ToList() )
                    {
                        ddlAssessmentType.Items.Add( new ListItem( workflowType.Name, workflowType.Id.ToString() ) );
                    }
                }
                ddlAssessmentType.SetValue( gfAssessmentFilter.GetUserPreference( "Assessment Type" ) );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var assessmentQry = new AssessmentService( rockContext )
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

                    assessmentQry = assessmentQry.Where( v => personIds.Contains( v.PersonAlias.PersonId ) );
                }
                else
                {
                    var dateTimeField = gAssessments.ColumnsOfType<RockBoundField>().Where( c => c.HeaderText == "Intake Date/Time" ).First();
                    var timeField = gAssessments.ColumnsOfType<RockBoundField>().Where( c => c.HeaderText == "Intake Time" ).First();

                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( gfAssessmentFilter.GetUserPreference( "Assessment Date" ) );
                    if ( dateRange.Start.HasValue || dateRange.End.HasValue )
                    {
                        dateTimeField.Visible = true;
                        timeField.Visible = false;

                        if ( dateRange.Start.HasValue )
                        {
                            assessmentQry = assessmentQry
                                .Where( v => v.AssessmentDate >= dateRange.Start.Value );
                        }
                        if ( dateRange.End.HasValue )
                        {
                            assessmentQry = assessmentQry
                                .Where( v => v.AssessmentDate < dateRange.End.Value );
                        }
                    }
                    else
                    {
                        dateTimeField.Visible = false;
                        timeField.Visible = true;

                        var today = RockDateTime.Today;
                        var tomorrow = today.AddDays( 1 );
                        assessmentQry = assessmentQry
                            .Where( v =>
                                v.AssessmentDate >= today &&
                                v.AssessmentDate < tomorrow );
                    }

                    bool includeCompleted = gfAssessmentFilter.GetUserPreference( "Include Completed" ).AsBoolean();
                    if ( !includeCompleted )
                    {
                        assessmentQry = assessmentQry
                            .Where( v => v.Workflows.Any( w => w.CompletedDateTime == null ) );
                    }

                    string firstName = gfAssessmentFilter.GetUserPreference( "First Name" );
                    if ( !string.IsNullOrWhiteSpace( firstName ) )
                    {
                        assessmentQry = assessmentQry
                            .Where( v =>
                                v.PersonAlias.Person.NickName.StartsWith( firstName ) ||
                                v.PersonAlias.Person.FirstName.StartsWith( firstName ) );
                    }

                    string lastName = gfAssessmentFilter.GetUserPreference( "Last Name" );
                    if ( !string.IsNullOrWhiteSpace( lastName ) )
                    {
                        assessmentQry = assessmentQry
                            .Where( v => v.PersonAlias.Person.LastName.StartsWith( lastName ) );
                    }

                    int? assessmentType = gfAssessmentFilter.GetUserPreference( "Assessment Type" ).AsIntegerOrNull();
                    if ( assessmentType.HasValue )
                    {
                        assessmentQry = assessmentQry
                            .Where( v => v.Workflows.Any( w => w.WorkflowTypeId == assessmentType.Value ) );
                    }

                }

                var sortProperty = gAssessments.SortProperty;
                if ( sortProperty != null )
                {
                    gAssessments.SetLinqDataSource<Assessment>( assessmentQry.Sort( sortProperty ) );
                }
                else
                {
                    gAssessments.SetLinqDataSource<Assessment>( assessmentQry.OrderBy( v => v.AssessmentDate ) );
                }

                _assessmentsWithNotes = new List<int>();
                var currentAssessments = gAssessments.DataSource as List<Assessment>;
                if ( currentAssessments != null )
                {
                    var assessmentIds = currentAssessments.Select( v => v.Id ).ToList();
                    
                    // Get assessments with any notes
                    var assessmentEntityType = EntityTypeCache.Read( typeof(Assessment) );
                    if ( assessmentEntityType != null )
                    {
                        _assessmentsWithNotes = new NoteService( rockContext ).Queryable().AsNoTracking()
                            .Where( n =>
                                n.NoteType.EntityTypeId == assessmentEntityType.Id &&
                                n.EntityId.HasValue &&
                                assessmentIds.Contains( n.EntityId.Value ) )
                            .Select( n => n.EntityId.Value )
                            .ToList()
                            .Distinct()
                            .ToList();
                    }
                }

                gAssessments.DataBind();

            }
        }

        #endregion

    }
}