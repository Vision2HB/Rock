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
    [DisplayName( "Dashboard" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center Service Area Dashboard." )]

    [CustomCheckboxListField( "Service Areas", "The service areas to display", @"
        SELECT 
	        CAST([Guid] AS VARCHAR(50)) AS [Value], 
	        [Name] AS [Text] 
        FROM [_org_willowcreek_CareCenter_ServiceArea] 
        ORDER BY [Order]
",
        true, "", "", 0 )]
    [BooleanField( "Display Language Column", "Should the person's language preference be displayed?", false, "", 1 )]
    [LinkedPage( "Workflow Entry Page", "Page to direct user to when they click a service.", true, "", "", 2 )]
    [LinkedPage( "Person Profile Page", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", false, "", "", 3, "PersonProfilePage" )]
    public partial class Dashboard : Rock.Web.UI.RockBlock
    {

        #region Fields

        private List<int> _workflowTypeIds = new List<int>();
        private List<Visit> _visits = new List<Visit>();
        private bool _showLanguage = false;

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

            lIcon.Text = string.Format( "<i class='{0}'></i>", this.RockPage.PageIcon );

            gFilter.ApplyFilterClick += gFilter_ApplyFilterClick;
            gFilter.ClearFilterClick += gFilter_ClearFilterClick;
            gFilter.DisplayFilterValue += gFilter_DisplayFilterValue;

            gVisits.DataKeyNames = new string[] { "Id" };
            gVisits.Actions.ShowAdd = false;
            gVisits.RowDataBound += GVisits_RowDataBound;
            gVisits.GridRebind += gVisits_GridRebind;
            gVisits.IsDeleteEnabled = true;
            gVisits.ShowConfirmDeleteDialog = false;

            // Get the valid service areas
            var validAreas = GetAttributeValue( "ServiceAreas" ).SplitDelimitedValues().AsGuidList();
            using ( var rockContext = new RockContext() )
            {
                _workflowTypeIds = new ServiceAreaService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a => validAreas.Contains( a.Guid ) && a.WorkflowTypeId.HasValue )
                    .Select( a => a.WorkflowTypeId.Value )
                    .ToList();
            }

            AddAttributeColumns();

            var editField = new LinkButtonField();
            editField.CssClass = "btn btn-sm btn-success dashboard-waiting-button";
            editField.Text = "<i class='fa fa-sign-in'></i>";
            editField.ToolTip = "Update To Waiting";
            editField.HeaderText = "Set to Waiting";
            gVisits.Columns.Add( editField );
            editField.Click += gVisits_Waiting;

            var deleteField = new DeleteField();
            deleteField.ButtonCssClass = "btn btn-sm btn-warning dashboard-cancel-button";
            deleteField.IconCssClass = "fa fa-minus";
            deleteField.Tooltip = "Cancel";
            deleteField.HeaderText = "Cancel";
            gVisits.Columns.Add( deleteField );
            deleteField.Click += gVisits_Delete;

            _showLanguage = GetAttributeValue( "DisplayLanguageColumn" ).AsBoolean();

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
                string key = string.Format( "dashboard-include-pending-{0}", this.BlockId );

                BindFilter();
                BindGrid();
            }
            else
            {
                ShowDialog();
            }
        }

        #endregion

        #region Events

        private void gFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            gFilter.SaveUserPreference( "First Name", tbFirstName.Text );
            gFilter.SaveUserPreference( "Last Name", tbLastName.Text );

            BindGrid();
        }

        private void gFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Status":
                case "First Name":
                case "Last Name":
                    {
                        break;
                    }
                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        private void gFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gFilter.DeleteUserPreferences();
            BindFilter();
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

        protected void cbFilter_CheckedChanged( object sender, EventArgs e )
        {
            gFilter.SaveUserPreference( "Show Pending", cbShowPending.Checked.ToString() );
            gFilter.SaveUserPreference( "Show Complete", cbShowComplete.Checked.ToString() );
            BindGrid();
        }

        private void GVisits_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                Workflow workflow = e.Row.DataItem as Workflow;
                if ( workflow != null )
                {
                    var visit = _visits.Where( v => v.Workflows.Any( vw => vw.Id == workflow.Id ) ).FirstOrDefault();

                    // Start Time
                    var lStartTime = e.Row.FindControl( "lStartTime" ) as Literal;
                    if ( lStartTime != null )
                    {
                        DateTime? startDateTime = visit != null ? visit.VisitDate : workflow.ActivatedDateTime;
                        if ( startDateTime.HasValue )
                        {
                            if ( startDateTime.Value > RockDateTime.Today )
                            {
                                lStartTime.Text = startDateTime.Value.ToShortTimeString();
                            }
                            else
                            {
                                lStartTime.Text = startDateTime.Value.ToShortDateTimeString();
                            }
                        }
                    }

                    Person person = null;
                    int? personId = null;
                    string personName = string.Empty;
                    if ( visit != null && visit.PersonAlias != null && visit.PersonAlias.Person != null )
                    {
                        person = visit.PersonAlias.Person;
                    }
                    else
                    {
                        workflow.LoadAttributes();
                        var personAliasGuid = workflow.GetAttributeValue( "Person" ).AsGuidOrNull();
                        if ( personAliasGuid.HasValue )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                person = new PersonAliasService( rockContext ).GetPerson( personAliasGuid.Value );
                            }
                        }
                    }

                    if ( person != null )
                    {
                        personId = person.Id;
                        personName = person.FullName;

                        if ( _showLanguage )
                        {
                            if ( person.Attributes == null )
                            {
                                person.LoadAttributes();
                            }
                            var lLanguage = e.Row.FindControl( "lLanguage" ) as Literal;
                            var languageAttr = AttributeCache.Read( org.willowcreek.CareCenter.SystemGuid.Attribute.PERSON_PREFERRED_LANGUAGE.AsGuid() );
                            if ( lLanguage != null && languageAttr != null )
                            {
                                lLanguage.Text = languageAttr.FieldType.Field.FormatValue( null, person.GetAttributeValue( languageAttr.Key ), languageAttr.QualifierValues, false ); 
                            }
                        }
                    }

                    // Primary Contact
                    var lPrimaryContact = e.Row.FindControl( "lPrimaryContact" ) as Literal;
                    if ( lPrimaryContact != null )
                    {
                        lPrimaryContact.Text = personName;
                    }

                    // Profile Link
                    var aProfileLink = e.Row.FindControl( "aProfileLink" ) as HtmlAnchor;
                    if ( aProfileLink != null )
                    {
                        if ( personId.HasValue )
                        {
                            var qryParams = new Dictionary<string, string> { { "PersonId", personId.Value.ToString() } };
                            aProfileLink.HRef = LinkedPageUrl( "PersonProfilePage", qryParams );
                            aProfileLink.Visible = true;
                        }
                        else
                        {
                            aProfileLink.Visible = false;
                        }
                    }

                    if ( visit != null )
                    {
                        // Pager Id
                        var lPager = e.Row.FindControl( "lPager" ) as Literal;
                        if ( lPager != null )
                        {
                            lPager.Text = visit.PagerId.HasValue ? visit.PagerId.Value.ToString() : "";
                        }

                        // Other Service Areas
                        var lOtherServiceAreas = e.Row.FindControl( "lOtherServiceAreas" ) as Literal;
                        if ( lOtherServiceAreas != null )
                        {
                            var serviceAreas = new StringBuilder();
                            foreach ( var otherWorkflow in visit.Workflows
                                .Where( w => w.WorkflowTypeId != workflow.WorkflowTypeId )
                                .OrderBy( w => w.WorkflowType.Order ) )
                            {
                                string qualifier = string.Empty;
                                switch ( otherWorkflow.Status )
                                {
                                    case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_COMPLETED: qualifier = " (Complete)"; break;
                                    case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_CANCELLED: qualifier = " (Cancelled)"; break;
                                    default: qualifier = string.Empty; break;
                                }
                                serviceAreas.AppendFormat( "<div class=''>{0} {1}</div>", otherWorkflow.WorkflowType.Name, qualifier );
                            }

                            lOtherServiceAreas.Text = serviceAreas.ToString();
                        }

                        // First Visit
                        var lFirstVisit = e.Row.FindControl( "lFirstVisit" ) as Literal;
                        if ( lFirstVisit != null && visit.PersonAlias != null )
                        {
                            if ( visit.FirstVisit() )
                            { 
                                lFirstVisit.Text = "<i class='fa fa-check'></i>";
                            }
                        }
                    }

                    // Status
                    var lStatus = e.Row.FindControl( "lStatus" ) as Literal;
                    if ( lStatus != null )
                    {
                        if ( workflow.Attributes == null )
                        {
                            workflow.LoadAttributes();
                        }

                        string labelClass = workflow.GetAttributeValue( "StatusLabelClass" );
                        if ( string.IsNullOrWhiteSpace( labelClass ) )
                        {
                            switch ( workflow.Status )
                            {
                                case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_PENDING:
                                    labelClass = "default";
                                    break;
                                case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_CANCELLED:
                                    labelClass = "danger";
                                    break;
                                default:
                                    labelClass = "success";
                                    break;
                            }
                        }
                        string label = string.Format( "<span class='label label-{0}'>{1}</span>", labelClass, workflow.Status );
                        lStatus.Text = string.Format( "<div class=''>{0} {1}</div>", workflow.WorkflowType.Name, label );
                    }

                    foreach( var lb in e.Row.ControlsOfTypeRecursive<LinkButton>() )
                    {
                        lb.Attributes["data-visit-type"] = workflow.WorkflowType.Name;
                    }

                    if ( workflow.Status != "Pending")
                    {
                        e.Row.Cells[e.Row.Cells.Count - 2].Controls.Clear();
                    }

                    if ( !workflow.IsActive )
                    {
                        e.Row.Cells[e.Row.Cells.Count - 1].Controls.Clear();
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

        protected void gVisits_RowSelected( object sender, RowEventArgs e )
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

        protected void gVisits_Waiting( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var workflowService = new WorkflowService( rockContext );
                var workflow = workflowService.Get( e.RowKeyId );
                if ( workflow != null )
                {
                    workflow.Status = "Waiting";
                    List<string> workflowErrors;
                    workflowService.Process( workflow, out workflowErrors );
                }
            }

            BindGrid();
        }

        protected void gVisits_Delete( object sender, RowEventArgs e )
        {
            hfWorkflowId.Value = e.RowKeyId.ToString();
            ddlDlgCancelWorkflowReason.BindToDefinedType( DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.VISIT_CANCEL_REASON.AsGuid() ), true );
            ShowDialog( "CancelWorkFlow" );
        }

        protected void dlgCancelWorkflow_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var workflowService = new WorkflowService( rockContext );
                var workflow = workflowService.Get( hfWorkflowId.Value.AsInteger() );
                if ( workflow != null )
                {
                    workflow.LoadAttributes( rockContext );
                    Guid? nextWorkflowGuid = workflow.GetAttributeValue( "NextWorkflow" ).AsGuidOrNull();

                    var definedValue = DefinedValueCache.Read( ddlDlgCancelWorkflowReason.SelectedValueAsInt() ?? 0 );
                    if ( definedValue != null )
                    {
                        workflow.SetAttributeValue( "CancelReason", definedValue.Guid.ToString().ToLower() );
                    }

                    workflow.AddLogEntry( "Service Area Cancelled" );
                    workflow.CompletedDateTime = RockDateTime.Now;
                    workflow.Status = org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_CANCELLED;

                    rockContext.SaveChanges();
                    workflow.SaveAttributeValues( rockContext );

                    if ( nextWorkflowGuid.HasValue )
                    {
                        var nextWorkflow = workflowService.Get( nextWorkflowGuid.Value );
                        if ( nextWorkflow != null &&
                            nextWorkflow.Status == "Pending" &&
                            !nextWorkflow.CompletedDateTime.HasValue )
                        {
                            nextWorkflow.Status = "Waiting";
                            List<string> workflowErrors;
                            workflowService.Process( nextWorkflow, out workflowErrors );
                        }
                    }

                    // If all the workflows have been completed for this visit...
                    foreach ( var visit in new VisitService( rockContext ).Queryable()   // should only be one visit
                        .Where( v => 
                            v.Workflows.Any( w => w.Id == workflow.Id) && 
                            !v.Workflows.Any( w => w.CompletedDateTime == null ) )
                        .ToList() )
                    {
                        // check to see if any do not have a status of Cancelled
                        if ( visit.Workflows.Any( w => w.Status != org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_CANCELLED ) )
                        {
                            // If so, mark the visit complete
                            visit.Status = VisitStatus.Complete;
                        }
                        else
                        {
                            // otherwise mark it cancelled
                            visit.Status = VisitStatus.Cancelled;
                        }

                        rockContext.SaveChanges();
                    }
                }
            }

            HideDialog();

            BindGrid();
        }

        #endregion

        #region Methods

        protected void AddAttributeColumns()
        {
            // Remove attribute columns
            foreach ( var column in gVisits.Columns.OfType<AttributeField>().ToList() )
            {
                gVisits.Columns.Remove( column );
            }

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
                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gVisits.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = dataFieldExpression;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Read( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gVisits.Columns.Add( boundField );
                    }
                }
            }
        }

        private void BindFilter()
        {
            ddlStatus.Items.Clear();
            ddlStatus.Items.Add( new ListItem() );
            using ( var rockContext = new RockContext() )
            {
                var validWorkflowTypeGuids = GetAttributeValue( "Workflows" ).SplitDelimitedValues().AsGuidList();
                foreach ( var status in new WorkflowService( rockContext ).Queryable().AsNoTracking()
                    .Where( w => _workflowTypeIds.Contains( w.WorkflowTypeId) )
                    .Select( w => w.Status )
                    .Distinct()
                    .ToList()
                    .OrderBy( s => s )
                    .ToList() )
                {
                    ddlStatus.Items.Add( new ListItem( status, status ) );
                }
            }

            cbShowPending.Checked = gFilter.GetUserPreference( "Show Pending" ).AsBoolean();
            cbShowComplete.Checked = gFilter.GetUserPreference( "Show Complete" ).AsBoolean();
            ddlStatus.SetValue( gFilter.GetUserPreference( "Status" ) );
            tbFirstName.Text = gFilter.GetUserPreference( "First Name" );
            tbLastName.Text = gFilter.GetUserPreference( "Last Name" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {

            using ( var rockContext = new RockContext() )
            {
                var qry = new WorkflowService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( w =>
                        _workflowTypeIds.Contains( w.WorkflowTypeId ) &&
                        w.ActivatedDateTime.HasValue );

                if ( !gFilter.GetUserPreference( "Show Pending" ).AsBoolean() )
                {
                    qry = qry.Where( w => w.Status != "Pending" );
                }

                var status = gFilter.GetUserPreference( "Status" );
                if ( status.IsNotNullOrWhitespace() )
                {
                    qry = qry.Where( w => w.Status == status );
                }

                if ( gFilter.GetUserPreference( "Show Complete" ).AsBoolean() )
                {
                    var today = RockDateTime.Today;
                    qry = qry.Where( w =>
                        !w.CompletedDateTime.HasValue ||
                        w.CompletedDateTime.Value > today );
                }
                else
                { 
                    qry = qry.Where( w => 
                        !w.CompletedDateTime.HasValue );
                }

                var firstName = gFilter.GetUserPreference( "First Name" );
                var lastName = gFilter.GetUserPreference( "Last Name" );
                if ( firstName.IsNotNullOrWhitespace() || lastName.IsNotNullOrWhitespace() )
                {
                    var workflowEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Workflow ) ).Id;
                    var qualifiers = new List<string>();
                    _workflowTypeIds.ForEach( i => qualifiers.Add( i.ToString() ) );
                    var personAttributeIds = new AttributeService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.EntityTypeId == workflowEntityTypeId &&
                            a.EntityTypeQualifierColumn == "WorkflowTypeId" &&
                            qualifiers.Contains( a.EntityTypeQualifierValue ) )
                        .Select( a => a.Id )
                        .ToList();

                    if ( personAttributeIds.Any() )
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

                        var workflowIdQry = new AttributeValueService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( v =>
                                personAttributeIds.Contains( v.AttributeId ) &&
                                personAliasIds.Contains( v.Value ) )
                            .Select( v => v.EntityId )
                            .ToList();

                        qry = qry.Where( w => workflowIdQry.Contains( w.Id ) );
                    }
                }

                var workflows = qry.ToList();

                // Get the workflow ids
                var workflowIds = qry.Select( w => w.Id ).ToList();

                // Find all the visits that are associated with any of those workflows
                _visits = new VisitService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( v => v.Workflows.Any( w => workflowIds.Contains( w.Id ) ) )
                    .OrderBy( v => v.VisitDate )
                    .ToList();

                 // Set column visibility
                gVisits.ColumnsOfType<RockBoundField>().Where( c => c.HeaderText == "Type" ).First().Visible = _workflowTypeIds.Count > 1;
                gVisits.ColumnsOfType<RockTemplateField>().Where( c => c.HeaderText == "Language" ).First().Visible = _showLanguage;

                var sortProperty = gVisits.SortProperty;
                if ( sortProperty != null )
                {
                    if ( sortProperty.Property == "Pager" )
                    {
                        var wfPagerIds = _visits
                            .SelectMany( v => v.Workflows, ( v, w ) => new WorkflowPagerId( w.Id, v.PagerId ) )
                            .ToList();

                        workflowIds
                            .Where( i => 
                                !wfPagerIds.Select( p => p.WorkflowId ).Contains( i ) )
                            .ToList()
                            .ForEach( i => wfPagerIds.Add( new WorkflowPagerId( i, 0 ) ) );

                        var workflowPagers = workflows
                            .Join( wfPagerIds, w => w.Id, p => p.WorkflowId, ( w, p ) => new
                            {
                                Workflow = w,
                                PagerId = p.PagerId
                            } )
                            .ToList();


                        if ( sortProperty.Direction == SortDirection.Ascending )
                        {
                            gVisits.DataSource = workflowPagers
                                .OrderBy( w => w.PagerId )
                                .Select( w => w.Workflow )
                                .ToList();
                        }
                        else
                        {
                            gVisits.DataSource = workflowPagers
                                .OrderByDescending( w => w.PagerId )
                                .Select( w => w.Workflow )
                                .ToList();
                        }
                    }
                    else
                    {
                        gVisits.DataSource = qry.AsQueryable().Sort( sortProperty ).ToList();
                    }
                }
                else
                {
                    gVisits.DataSource = qry.OrderBy( w => w.ActivatedDateTime.Value ).ToList();
                }

                gVisits.DataBind();
            }
        }

        private void RegisterJavaScript()
        {
            string gridButtonScriptFormat = @"
$('#{0} .dashboard-cancel-button').not('.disabled').on( 'click', function (e) {{
    e.preventDefault();
    Rock.dialogs.confirm('Are you sure you want to cancel this ' + $(this).attr('data-visit-type') + '?', function (result) {{
        if (result) {{
            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
        }}
    }});
}});

$('#{0} .dashboard-waiting-button').not('.disabled').on( 'click', function (e) {{
    e.preventDefault();
    Rock.dialogs.confirm('Are you sure you want to override the status of this ' + $(this).attr('data-visit-type') + ' to Waiting?', function (result) {{
        if (result) {{
            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
        }}
    }});
}});";
            string gridButtonScript = string.Format( gridButtonScriptFormat, gVisits.ClientID );
            ScriptManager.RegisterStartupScript( gVisits, gVisits.GetType(), "dashboard-grid-confirm-" + this.ClientID, gridButtonScript, true );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "CANCELWORKFLOW":
                    dlgCancelWorkflow.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "CANCELWORKFLOW":
                    dlgCancelWorkflow.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion

        public class WorkflowPagerId
        {
            public int WorkflowId { get; set; }
            public int? PagerId { get; set; }
            public WorkflowPagerId ( int workflowId, int? pagerId )
            {
                WorkflowId = workflowId;
                PagerId = pagerId;
            }
        }
    }
}