using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
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
    [DisplayName( "Assessment Dashboard" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center Assessment Dashboard." )]

    [CustomCheckboxListField( "Workflows", "The workflow types to display", @"
        SELECT 
	        CAST(W.[Guid] AS VARCHAR(50)) AS [Value], 
	        W.[Name] AS [Text] 
        FROM [WorkflowType] W
        INNER JOIN [Category] C ON C.[Id] = W.[CategoryId]
        WHERE C.[Guid] = '11601938-4335-41FA-9D44-04CA1054649E'
        ORDER BY W.[Order]
",
        true, "", "", 0 )]
    [LinkedPage( "Workflow Entry Page", "Page to direct user to when they click a service.", true, "", "", 2 )]
    [LinkedPage( "Person Profile Page", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", false, "", "", 3, "PersonProfilePage" )]
    [BooleanField( "Show Campus Filter", "Should a campus filter be displayed (this requires the workflow to have a campus attribute).", false, "", 4)]
    [BooleanField( "Show Id Column", "Should the ID column be displayed?", false, "", 5)]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Alert Attribute", "If this attribute is selected (must be boolean field type), the profile link will be displayed as red.", false, false, "", "", 6 )]
    [NoteTypeField("Warning Note Types", "If person has any of these types of notes, the profile link will be displayed as yellow.", true, "Rock.Model.Person", "", "", false, "", "", 7)]
    [TextField("Cancel Confirmation", "Confirmation Message to display when user cancels a request. Leave blank to display message based on configured workflow type", false, "", "", 8)]
    public partial class AssessmentDashboard : Rock.Web.UI.RockBlock
    {

        #region Fields

        private List<int> _workflowTypeIds = new List<int>();
        private Dictionary<int, Person> _workflowPerson = new Dictionary<int, Person>();
        private List<int> _alertPersonIds = new List<int>();
        private List<int> _warningPersonIds = new List<int>();

        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AvailableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;

            AddDynamicControls();
        }

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

            lIcon.Text = string.Format( "<i class='{0}'></i>", this.RockPage.PageIcon );

            cbShowComplete.Checked = this.GetBlockUserPreference( "complete" ).AsBoolean();

            gFilter.ApplyFilterClick += gFilter_ApplyFilterClick;
            gFilter.ClearFilterClick += gFilter_ClearFilterClick;
            gFilter.DisplayFilterValue += gFilter_DisplayFilterValue;

            gAssessments.DataKeyNames = new string[] { "Id" };
            gAssessments.Actions.ShowAdd = false;
            gAssessments.RowDataBound += GAssessments_RowDataBound;
            gAssessments.GridRebind += gAssessments_GridRebind;
            gAssessments.IsDeleteEnabled = true;
            gAssessments.ShowConfirmDeleteDialog = false;
            gAssessments.IsDeleteEnabled = UserCanEdit;

            // Get the valid service areas
            var validWorkflowGuids = GetAttributeValue( "Workflows" ).SplitDelimitedValues().AsGuidList();
            using ( var rockContext = new RockContext() )
            {
                var workflowTypes = new WorkflowTypeService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( w => validWorkflowGuids.Contains( w.Guid ) )
                    .ToList();

                _workflowTypeIds = workflowTypes.Select( t => t.Id ).ToList();

                ddlAssessmentType.Items.Clear();
                ddlAssessmentType.Items.Add( new ListItem( "All", "" ) );
                foreach( var workflowType in workflowTypes)
                {
                    ddlAssessmentType.Items.Add( new ListItem( workflowType.Name, workflowType.Id.ToString() ) );
                }
                ddlAssessmentType.Visible = workflowTypes.Count > 1;
                ddlAssessmentType.SetValue( this.GetBlockUserPreference( "type" ) );
            }

            if ( GetAttributeValue( "ShowCampusFilter" ).AsBoolean() )
            {
                var campusi = CampusCache.All();
                cpCampus.Campuses = campusi;
                cpCampus.Visible = campusi.Any();
                cpCampus.SetValue( this.GetBlockUserPreference( "campus" ) );
            }
            else
            {
                cpCampus.Visible = false;
            }

            RegisterJavaScript();
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
                BindGrid();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["AvailableAttributes"] = AvailableAttributes;

            return base.SaveViewState();
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
            BindGrid();
        }

        private void gFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            gFilter.SaveUserPreference( "First Name", tbFirstName.Text );
            gFilter.SaveUserPreference( "Last Name", tbLastName.Text );
            gFilter.SaveUserPreference( "Started", drpDate.DelimitedValues );

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    if ( filterControl != null )
                    {
                        try
                        {
                            var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                            gFilter.SaveUserPreference( attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                        }
                        catch
                        {
                            // intentionally ignore
                        }
                    }
                }
            }
            BindGrid();
        }

        private void gFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( AvailableAttributes != null )
            {
                var attribute = AvailableAttributes.FirstOrDefault( a => a.Key == e.Key );
                if ( attribute != null )
                {
                    try
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                        e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
                        return;
                    }
                    catch
                    {
                        // intentionally ignore
                    }
                }
            }

            if ( e.Key == "Status" || e.Key == "First Name" || e.Key == "Last Name" )
            {
                return;
            }
            else if ( e.Key == "Started" )
            {
                e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        private void gFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gFilter.DeleteUserPreferences();
            BindFilter();
        }

        protected void Filter_Changed( object sender, EventArgs e )
        {
            this.SetBlockUserPreference( "complete", cbShowComplete.Checked.ToString() );
            this.SetBlockUserPreference( "type", ddlAssessmentType.SelectedValue );
            this.SetBlockUserPreference( "campus", cpCampus.SelectedValue );

            BindGrid();
        }

        private void GAssessments_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                Workflow workflow = e.Row.DataItem as Workflow;
                if ( workflow != null )
                {
                    // Start Time
                    var lStartTime = e.Row.FindControl( "lStartTime" ) as Literal;
                    if ( lStartTime != null )
                    {
                        DateTime? startDateTime = workflow.ActivatedDateTime;
                        if ( startDateTime.HasValue )
                        {
                            lStartTime.Text = startDateTime.Value.ToShortDateTimeString();
                        }
                    }

                    var person = _workflowPerson.Where( w => w.Key == workflow.Id ).Select( w => w.Value ).FirstOrDefault();

                    // Primary Contact
                    var lPrimaryContact = e.Row.FindControl( "lPrimaryContact" ) as Literal;
                    if ( lPrimaryContact != null )
                    {
                        lPrimaryContact.Text = person != null ? person.FullName : "[unknown]";
                    }

                    // Profile Link
                    var aProfileLink = e.Row.FindControl( "aProfileLink" ) as HtmlAnchor;
                    if ( aProfileLink != null )
                    {
                        if ( person != null )
                        {
                            var qryParams = new Dictionary<string, string> { { "PersonId", person.Id.ToString() } };
                            aProfileLink.HRef = LinkedPageUrl( "PersonProfilePage", qryParams );
                            if ( _alertPersonIds.Contains( person.Id ) )
                            {
                                aProfileLink.AddCssClass( "btn-danger" );
                            }
                            else if ( _warningPersonIds.Contains( person.Id ))
                            {
                                aProfileLink.AddCssClass( "btn-warning" );
                            }
                            else
                            {
                                aProfileLink.AddCssClass( "btn-default" );
                            }
                            aProfileLink.Visible = true;
                        }
                        else
                        {
                            aProfileLink.Visible = false;
                        }
                    }

                    foreach ( var lb in e.Row.ControlsOfTypeRecursive<LinkButton>() )
                    {
                        lb.Attributes["data-assessment-type"] = workflow.WorkflowType.WorkTerm;
                    }

                    if ( !workflow.IsActive )
                    {
                        e.Row.Cells[e.Row.Cells.Count - 1].Controls.Clear();
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

        protected void gAssessments_RowSelected( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                Workflow workflow = new WorkflowService( rockContext ).Get( e.RowKeyId );
                if ( workflow != null )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "WorkflowTypeId", workflow.WorkflowTypeId.ToString() );
                    qryParams.Add( "WorkflowId", workflow.Id.ToString() );
                    NavigateToLinkedPage( "WorkflowEntryPage", qryParams );
                }
            }
        }

        protected void gAssessments_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var workflow = new WorkflowService( rockContext ).Get( e.RowKeyId );
                if ( workflow != null )
                {
                    workflow.AddLogEntry( "Service Area Cancelled" );
                    workflow.CompletedDateTime = RockDateTime.Now;
                    workflow.Status = org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_CANCELLED;

                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        #endregion

        #region Methods

        private void BindFilter()
        {
            ddlStatus.Items.Clear();
            ddlStatus.Items.Add( new ListItem() );
            using ( var rockContext = new RockContext() )
            {
                var validWorkflowTypeGuids = GetAttributeValue( "Workflows" ).SplitDelimitedValues().AsGuidList();
                foreach ( var status in new WorkflowService( rockContext ).Queryable().AsNoTracking()
                    .Where( w => validWorkflowTypeGuids.Contains( w.WorkflowType.Guid ) )
                    .Select( w => w.Status )
                    .Distinct()
                    .ToList()
                    .OrderBy( s => s )
                    .ToList() )
                {
                    ddlStatus.Items.Add( new ListItem( status, status ) );
                }
            }

            ddlStatus.SetValue( gFilter.GetUserPreference( "Status" ) );
            tbFirstName.Text = gFilter.GetUserPreference( "First Name" );
            tbLastName.Text = gFilter.GetUserPreference( "Last Name" );
            drpDate.DelimitedValues = gFilter.GetUserPreference( "Started" );

            string json = gFilter.GetUserPreference( "SortOrder" ) as string;
            if ( !String.IsNullOrWhiteSpace( json ) )
            {
                gAssessments.SortProperty = JsonConvert.DeserializeObject<SortProperty>( json );
            }

            BindAttributes();
            AddDynamicControls();
        }

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            // Parse the attribute filters 
            AvailableAttributes = new List<AttributeCache>();
            if ( _workflowTypeIds != null )
            {
                // Add attribute columns
                int entityTypeId = new Workflow().TypeId;
                var qualifiers = new List<string>();
                _workflowTypeIds.ForEach( i => qualifiers.Add( i.ToString() ) );
                foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        qualifiers.Contains( a.EntityTypeQualifierValue ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    AvailableAttributes.Add( AttributeCache.Read( attribute ) );
                }
            }
        }

        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        private void AddDynamicControls()
        {
            // Clear the filter controls
            phAttributeFilters.Controls.Clear();

            // Remove attribute columns
            foreach ( var column in gAssessments.Columns.OfType<AttributeField>().ToList() )
            {
                gAssessments.Columns.Remove( column );
            }

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    if ( attribute.Key != "Campus" && attribute.Key != "Person" )
                    {
                        var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                        if ( control != null )
                        {
                            if ( control is IRockControl )
                            {
                                var rockControl = (IRockControl)control;
                                rockControl.Label = attribute.Name;
                                rockControl.Help = attribute.Description;
                                phAttributeFilters.Controls.Add( control );
                            }
                            else
                            {
                                var wrapper = new RockControlWrapper();
                                wrapper.ID = control.ID + "_wrapper";
                                wrapper.Label = attribute.Name;
                                wrapper.Controls.Add( control );
                                phAttributeFilters.Controls.Add( wrapper );
                            }

                            string savedValue = gFilter.GetUserPreference( attribute.Key );
                            if ( !string.IsNullOrWhiteSpace( savedValue ) )
                            {
                                try
                                {
                                    var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                                    attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, values );
                                }
                                catch
                                {
                                    // intentionally ignore
                                }
                            }
                        }
                    }

                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gAssessments.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = attribute.Key;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Read( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gAssessments.Columns.Add( boundField );
                    }
                }
            }


            var deleteField = new DeleteField();
            deleteField.ButtonCssClass = "btn btn-sm btn-warning dashboard-cancel-button";
            deleteField.IconCssClass = "fa fa-minus";
            //deleteField.Tooltip = "Cancel";
            deleteField.HeaderText = "Cancel";

            gAssessments.Columns.Add( deleteField );
            deleteField.Click += gAssessments_Delete;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var attributeValueService = new AttributeValueService( rockContext );

                // Set column visibility
                gAssessments.ColumnsOfType<RockBoundField>().Where( c => c.HeaderText == "Id" ).First().Visible = GetAttributeValue( "ShowIdColumn" ).AsBoolean();
                gAssessments.ColumnsOfType<RockBoundField>().Where( c => c.HeaderText == "Type" ).First().Visible = _workflowTypeIds.Count > 1;

                // Check to see if a campus filter was selected, and if so, hide any campus attribute column
                int? campusId = cpCampus.SelectedCampusId;
                gAssessments.ColumnsOfType<AttributeField>().Where( c => c.HeaderText == "Campus" ).ToList().ForEach( c => c.Visible = !campusId.HasValue );

                // Get the valid workflow types
                var workflowTypeIds = new List<int>( _workflowTypeIds );

                // If a workflow type was selected, only consider that type
                int? workflowTypeId = ddlAssessmentType.SelectedValueAsInt();
                if ( workflowTypeId.HasValue )
                {
                    workflowTypeIds = new List<int> { workflowTypeId.Value };
                }

                // Get the Campus and Person workflow attributes for the selected workflow types
                var workflowEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Workflow ) ).Id;
                var qualifiers = new List<string>();
                workflowTypeIds.ForEach( i => qualifiers.Add( i.ToString() ) );
                var wfAttributes = new AttributeService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.EntityTypeId == workflowEntityTypeId &&
                        a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        qualifiers.Contains( a.EntityTypeQualifierValue ) &&
                        ( a.Key == "Campus" || a.Key == "Person" ) )
                    .ToList();
                var campusAttributeIds = wfAttributes
                    .Where( a => a.Key == "Campus" )
                    .Select( a => a.Id )
                    .ToList();
                var personAttributeIds = wfAttributes
                    .Where( a => a.Key == "Person" )
                    .Select( a => a.Id )
                    .ToList();

                // Get the active workflows for the selected workflow type(s)
                var qry = new WorkflowService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( w =>
                        workflowTypeIds.Contains( w.WorkflowTypeId ) &&
                        w.ActivatedDateTime.HasValue );


                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( gFilter.GetUserPreference( "Started" ) );
                if ( dateRange.Start.HasValue || dateRange.End.HasValue )
                {
                    if ( dateRange.Start.HasValue )
                    {
                        qry = qry.Where( v => v.ActivatedDateTime >= dateRange.Start.Value );
                    }
                    if ( dateRange.End.HasValue )
                    {
                        qry = qry.Where( v => v.ActivatedDateTime < dateRange.End.Value );
                    }
                    
                    if ( !cbShowComplete.Checked )
                    {
                        qry = qry.Where( w => !w.CompletedDateTime.HasValue );
                    }
                }
                else
                {
                    // If not showing complete, filter those that are completed
                    if ( cbShowComplete.Checked )
                    {
                        var today = RockDateTime.Today;
                        qry = qry.Where( w => !w.CompletedDateTime.HasValue || w.CompletedDateTime.Value > today );
                    }
                    else
                    {
                        qry = qry.Where( w => !w.CompletedDateTime.HasValue );
                    }
                }

                // If campus id was entered, filter workflows by campus
                if ( campusId.HasValue && campusAttributeIds.Any() )
                {
                    var campus = CampusCache.Read( campusId.Value );
                    if ( campus != null )
                    {
                        string campusIdStr = campus.Id.ToString();
                        string campusGuidstr = campus.Guid.ToString();
                        var campusEntityIds = attributeValueService
                            .Queryable().AsNoTracking()
                            .Where( v =>
                                campusAttributeIds.Contains( v.AttributeId ) &&
                                ( v.Value == campusIdStr || v.Value == campusGuidstr )
                            )
                            .Select( v => v.EntityId );

                        qry = qry.Where( w => campusEntityIds.Contains( w.Id ) );
                    }
                }

                // If person name(s) were entered, filter by name
                var firstName = gFilter.GetUserPreference( "First Name" );
                var lastName = gFilter.GetUserPreference( "Last Name" );
                if ( ( !String.IsNullOrWhiteSpace( firstName ) || !String.IsNullOrWhiteSpace( lastName ) ) && personAttributeIds.Any() )
                {
                    var personIdQry = new PersonService( rockContext )
                        .GetByFirstLastName( firstName, lastName, false, false )
                        .AsNoTracking()
                        .Select( p => p.Id );

                    var personAliasIds = new PersonAliasService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p => personIdQry.Contains( p.PersonId ) )
                        .Select( p => p.Guid )
                        .ToList()
                        .Select( p => p.ToString() )
                        .ToList();

                    var workflowIdQry = attributeValueService
                        .Queryable().AsNoTracking()
                        .Where( v =>
                            personAttributeIds.Contains( v.AttributeId ) &&
                            personAliasIds.Contains( v.Value ) )
                        .Select( v => v.EntityId )
                        .ToList();

                    qry = qry.Where( w => workflowIdQry.Contains( w.Id ) );
                }

                // Filter by status
                var status = gFilter.GetUserPreference( "Status" );
                if ( !String.IsNullOrWhiteSpace( status ) )
                {
                    qry = qry.Where( w => w.Status == status );
                }

                // Filter query by any configured attribute filters
                if ( AvailableAttributes != null && AvailableAttributes.Any() )
                {
                    var parameterExpression = attributeValueService.ParameterExpression;

                    foreach ( var attribute in AvailableAttributes.Where( a => a.Key != "Campus" && a.Key != "Person" ) )
                    {
                        var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                        if ( filterControl != null )
                        {
                            var filterValues = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                            var expression = attribute.FieldType.Field.AttributeFilterExpression( attribute.QualifierValues, filterValues, parameterExpression );
                            if ( expression != null )
                            {
                                var attributeValues = attributeValueService
                                    .Queryable()
                                    .Where( v => v.Attribute.Id == attribute.Id );

                                attributeValues = attributeValues.Where( parameterExpression, expression, null );

                                qry = qry.Where( w => attributeValues.Select( v => v.EntityId ).Contains( w.Id ) );
                            }
                        }
                    }
                }

                // Execute the query to get in-memory list of workflows
                var workflows = qry.ToList();

                var workflowIds = workflows.Select( w => w.Id ).ToList();

                // Find all the assessments that are associated with any of those workflows
                _workflowPerson = new Dictionary<int, Person>();
                foreach( var wfPerson in new AssessmentService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a => a.Workflows.Any( w => workflowIds.Contains( w.Id ) ) )
                    .Select( a => new {
                        WorkflowIds = a.Workflows.Select( w => w.Id ).ToList(),
                        Person = a.PersonAlias.Person
                    } )
                    .ToList() )
                {
                    foreach( var wfId in wfPerson.WorkflowIds )
                    {
                        _workflowPerson.AddOrIgnore( wfId, wfPerson.Person );
                    }
                }

                // Add the people for workflows that did not have an assessment
                workflowIds = workflowIds.Where( w => !_workflowPerson.Keys.Contains( w ) ).ToList();
                if ( workflowIds.Any() )
                {
                    // Get the person attribute values from those workflows
                    var workflowPersonAliasGuids = attributeValueService
                        .Queryable().AsNoTracking()
                        .Where( v =>
                            personAttributeIds.Contains( v.AttributeId ) &&
                            v.EntityId.HasValue &&
                            workflowIds.Contains( v.EntityId.Value ) )
                        .ToList()
                        .ToDictionary( k => k.EntityId.Value, v => v.Value.AsGuid() );

                    // Add the people associated to those workflows
                    var personAliasGuids = workflowPersonAliasGuids.Select( w => w.Value ).ToList();
                    var personAliasPeople = new PersonAliasService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a => personAliasGuids.Contains( a.Guid ) )
                        .ToDictionary( k => k.Guid, v => v.Person );

                    foreach( var wfPa in workflowPersonAliasGuids )
                    {
                        if ( personAliasPeople.ContainsKey( wfPa.Value ) )
                        {
                            _workflowPerson.AddOrIgnore( wfPa.Key, personAliasPeople[wfPa.Value] );
                        }
                    }
                }

                // Get list of person ids for selected workflows
                var personIds = _workflowPerson.Select( w => w.Value.Id ).ToList();

                // Find the person ids with an alert
                _alertPersonIds = new List<int>();
                Guid? alertAttributeGuid = GetAttributeValue( "AlertAttribute" ).AsGuidOrNull();
                if ( alertAttributeGuid.HasValue )
                {
                    _alertPersonIds = attributeValueService
                        .Queryable().AsNoTracking()
                        .Where( v =>
                            v.Attribute.Guid == alertAttributeGuid.Value &&
                            v.EntityId.HasValue &&
                            personIds.Contains( v.EntityId.Value ) &&
                            v.Value != null &&
                            v.Value != "" )
                        .ToList()
                        .Where( v => v.Value.AsBoolean() )
                        .Select( v => v.EntityId.Value )
                        .ToList();
                }

                // Find the person ids with a warning
                _warningPersonIds = new List<int>();
                List<Guid> warningNoteTypeGuids = GetAttributeValue( "WarningNoteTypes" ).SplitDelimitedValues().AsGuidList();
                if ( warningNoteTypeGuids.Any() )
                {
                    _warningPersonIds = new NoteService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( n =>
                            warningNoteTypeGuids.Contains( n.NoteType.Guid ) &&
                            n.EntityId.HasValue &&
                            personIds.Contains( n.EntityId.Value ) &&
                            !_alertPersonIds.Contains( n.EntityId.Value ) &&
                            n.Text != null &&
                            n.Text != "" )
                        .Select( n => n.EntityId.Value )
                        .ToList();
                }

                // Sort and bind the data.
                var sortProperty = gAssessments.SortProperty;
                if ( sortProperty != null )
                {
                    gFilter.SaveUserPreference( "SortOrder", gAssessments.SortProperty.ToJson() );
                    gAssessments.DataSource = workflows.AsQueryable().Sort( sortProperty ).ToList(); 
                }
                else
                {
                    gAssessments.DataSource = workflows.OrderBy( w => w.ActivatedDateTime.Value ).ToList();
                }

                gAssessments.DataBind();
            }
        }

        private void RegisterJavaScript()
        {
            string confirmationMsg = GetAttributeValue( "CancelConfirmation" );
            if ( String.IsNullOrWhiteSpace( confirmationMsg ) )
            {
                confirmationMsg = "Are you sure you want to permanently cancel this ' + $(this).attr('data-assessment-type') + '?";
            }

            string deleteButtonScriptFormat = @"
$('#{0} .dashboard-cancel-button').not('.disabled').on( 'click', function (e) {{
    e.preventDefault();
    Rock.dialogs.confirm('{1}', function (result) {{
        if (result) {{
            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
        }}
    }});
}});";
            string deleteButtonScript = string.Format( deleteButtonScriptFormat, gAssessments.ClientID, confirmationMsg );
            ScriptManager.RegisterStartupScript( gAssessments, gAssessments.GetType(), "dashboard-delete-confirm-" + this.ClientID, deleteButtonScript, true );
        }

        #endregion

    }
}