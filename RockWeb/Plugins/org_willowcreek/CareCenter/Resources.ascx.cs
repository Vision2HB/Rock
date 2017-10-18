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

using DotLiquid;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Care Center block for viewing list of resources.
    /// </summary>
    [DisplayName( "Resources" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for viewing list of resources." )]

    [TextField("Search Caption (Left)", "Caption to display above the left search fields.", false, "", "", 0, "SearchCaptionLeft" )]
    [ValueListField( "Default Search Properties (Left)", "Properties that should always be visible to search on", true, "", "Property", "",
        @"
SELECT CAST(DT.[Id] AS VARCHAR) AS [Value], DT.[Name] AS [Text], DT.[Order] 
FROM [DefinedType] DT
INNER JOIN [Category] C ON C.[Id] = DT.[CategoryId]
WHERE C.[Guid] =  '7B305B81-17CD-48B5-AA19-DC29C382312B'
UNION ALL
SELECT 'ResourceName', 'Resource Name', 1000 UNION ALL
SELECT 'FirstName', 'Counselor''s First Name', 1001 UNION ALL
SELECT 'LastName', 'Counselor''s Last Name', 1002 UNION ALL 
SELECT 'PostalCode', 'Postal Code', 1003 UNION ALL
SELECT 'BenevolenceCounselor', 'Benevolence Counselor', 1004 UNION ALL
SELECT 'SupportGroupsOfferred', 'Support Groups Offerred', 1005 UNION ALL
SELECT 'SlidingFeeOffered', 'Sliding Fee Offered', 1006 UNION ALL
SELECT 'WillowAttender', 'Counselor Attends Willow', 1007 UNION ALL
SELECT 'ResourceType', 'Resource Type', 1008
ORDER BY [Order]
", "", 1, "SearchFieldsLeft" )]

    [TextField( "Search Caption (Right)", "Caption to display above the right search fields.", false, "", "", 2, "SearchCaptionRight" )]
    [ValueListField( "Default Search Properties (Right)", "Properties that should always be visible to search on", true, "", "Property", "",
        @"
SELECT CAST(DT.[Id] AS VARCHAR) AS [Value], DT.[Name] AS [Text], DT.[Order] 
FROM [DefinedType] DT
INNER JOIN [Category] C ON C.[Id] = DT.[CategoryId]
WHERE C.[Guid] =  '7B305B81-17CD-48B5-AA19-DC29C382312B'
UNION ALL
SELECT 'ResourceName', 'Resource Name', 1000 UNION ALL
SELECT 'FirstName', 'Counselor''s First Name', 1001 UNION ALL
SELECT 'LastName', 'Counselor''s Last Name', 1002 UNION ALL 
SELECT 'PostalCode', 'Postal Code', 1003 UNION ALL
SELECT 'BenevolenceCounselor', 'Benevolence Counselor', 1004 UNION ALL
SELECT 'SupportGroupsOfferred', 'Support Groups Offerred', 1005 UNION ALL
SELECT 'SlidingFeeOffered', 'Sliding Fee Offered', 1006 UNION ALL
SELECT 'WillowAttender', 'Counselor Attends Willow', 1007 UNION ALL
SELECT 'ResourceType', 'Resource Type', 1008
ORDER BY [Order]
", "", 3, "SearchFieldsRight" )]

    [ValueListField( "Additional Criteria Search Properties", "Properties that should only be visible in the additional criteria to search on.", false, "", "Property", "",
        @"
SELECT CAST(DT.[Id] AS VARCHAR) AS [Value], DT.[Name] AS [Text], DT.[Order] 
FROM [DefinedType] DT
INNER JOIN [Category] C ON C.[Id] = DT.[CategoryId]
WHERE C.[Guid] =  '7B305B81-17CD-48B5-AA19-DC29C382312B'
UNION ALL
SELECT 'ResourceName', 'Resource Name', 1000 UNION ALL
SELECT 'FirstName', 'Counselor''s First Name', 1001 UNION ALL
SELECT 'LastName', 'Counselor''s Last Name', 1002 UNION ALL 
SELECT 'PostalCode', 'Postal Code', 1003 UNION ALL
SELECT 'BenevolenceCounselor', 'Benevolence Counselor', 1004 UNION ALL
SELECT 'SupportGroupsOfferred', 'Support Groups Offerred', 1005 UNION ALL
SELECT 'SlidingFeeOffered', 'Sliding Fee Offered', 1006 UNION ALL
SELECT 'WillowAttender', 'Counselor Attends Willow', 1007 UNION ALL
SELECT 'ResourceType', 'Resource Type', 1008
ORDER BY [Order]
", "", 4, "AdditionalSearchFields" )]

    [CustomDropdownListField( "Required Property", "Only include resources that have one or more of the selected property values.",
        @"
SELECT CAST(DT.[Id] AS VARCHAR) AS [Value], DT.[Name] AS [Text], DT.[Order] 
FROM [DefinedType] DT
INNER JOIN [Category] C ON C.[Id] = DT.[CategoryId]
WHERE C.[Guid] =  '7B305B81-17CD-48B5-AA19-DC29C382312B'
ORDER BY [Order]
", false, "", "", 5 )]

    [CustomDropdownListField( "Audience", "The audience that is searching for resources. The results will be formatted differently based on the audience.", "PASTOR^Response Pastor,CC^Care Center", true, "PASTOR", "", 6 )]

    [CodeEditorField( "Details Format", "Lava to use when displaying the details of a resource", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, true, @"
<div class='row margin-b-sm'>
    <div class='col-md-12'>
        {{ Resource.Description }}
    </div>
</div>

<div class='row margin-b-md'>
    <div class='col-md-6'>
        <dl>
            {% if Resource.Website and Resource.Website != '' %}
                <dt>Website</dt>
                <dd><a href = '{{ Resource.Website }}' target='_blank'>{{ Resource.Website }}</a></dd>
            {% endif %}
            {% if Resource.PhoneNumber and Resource.PhoneNumber != '' %}
                <dt>Phone Number</dt>
                <dd>{{ Resource.PhoneNumber }}</dd>
            {% endif %}
            {% if Resource.MobileNumber and Resource.MobileNumber != '' %}
                <dt>Mobile Number</dt>
                <dd>{{ Resource.MobileNumber }}</dd>
            {% endif %}
            {% if Resource.HtmlAddress and Resource.HtmlAddress != '' %}
                <dt>Address</dt>
                <dd>{{ Resource.HtmlAddress }}</dd>
            {% endif %}
        </dl>
    </div>
    <div class='col-md-6'>
        <dl>
            {% if Resource.LastName and Resource.LastName != '' %}
                <dt>Name</dt>
                <dd>{{ Resource.FirstName }} {{ Resource.LastName }}</dd>
            {% endif %}
            {% if Resource.EmailAddress and Resource.EmailAddress != '' %}
                <dt>Email</dt>
                <dd>{{ Resource.EmailAddress }}</dd>
            {% endif %}
        </dl>
        {% if Resource.WillowAttender != null %}<div>Willow Attender: <strong>{% if Resource.WillowAttender == true %}Yes{% else %}No{% endif %}</strong></div>{% endif %}
        {% if Resource.SupportGroupsOfferred != null %}<div>Support Groups Offerred: <strong>{% if Resource.SupportGroupsOfferred == true %}Yes{% else %}No{% endif %}</strong></div>{% endif %}
        {% if Resource.ReducedFeeProgramParticpant != null %}<div>Benevolence Counselor: <strong>{% if Resource.ReducedFeeProgramParticpant == true %}Yes{% else %}No{% endif %}</strong></div>{% endif %}
        {% if Resource.SlidingFeeOffered != null %}<div>Sliding Fee Offerred: <strong>{% if Resource.SlidingFeeOffered == true %}Yes{% else %}No{% endif %}</strong></div>{% endif %}
    </div>
</div>

<div class='row'>
    <div class='col-md-12'>
        <dl>
            {% for propType in Resource.PropertyTypes %}
                <dt>{{ propType.PropertyType }}:</dt>
                <dd>{{ propType.Properties }}</dd>
            {% endfor %}
        </dl>
    </div>
</div>
", "", 7 )]

    [LinkedPage( "Print Page", "Page for printing details of the selected resource(s)", true, "", "", 8 )]
    [LinkedPage( "Communication Page", "Page for sending an email with details of the selected resource(s)", true, "", "", 9 )]
    [BooleanField( "Include Benevolence Option", "Should the Benevolence option be displayed?", false, "", 10 )]
    [LinkedPage( "Workflow Entry Page", "The page to return to after resources are printed (only applies if resources were started from a workflow).", true, "", "", 11 )]
    [CommunicationTemplateField( "Communication Template", "The communicatoin template to use when sending the resource email.", true, "", "", 12 )]
    public partial class Resources : Rock.Web.UI.RockBlock
    {
        #region Fields

        private LinkButton lbPrintPassport;
        private List<int> _appointmentsWithNotes;
        private Person _person;
        private int? _workflowId;
        private string _detailPageUrlFormat = string.Empty;
        private List<int> _validPropertyValues = new List<int>();
        private Dictionary<int, string> _selectedResources = new Dictionary<int, string>();
        private bool _responsePastorFormat = true;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _validPropertyValues = ViewState["ValidPropertyValues"] as List<int>;
            _selectedResources = ViewState["SelectedResources"] as Dictionary<int, string>;

            CreateDynamicControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _responsePastorFormat = GetAttributeValue( "Audience" ) != "CC";

            _detailPageUrlFormat = LinkedPageUrl( "DetailPage", new Dictionary<string, string> { { "ResourceId", "9999" } } ).Replace( "9999", "{0}" );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            rptrSelected.ItemCommand += RptrSelected_ItemCommand;

            rptrResults.ItemDataBound += RptrResults_ItemDataBound;
            rptrResults.ItemCommand += RptrResults_ItemCommand;

            int? personId = PageParameter( "PersonId" ).AsIntegerOrNull();
            if ( personId != null )
            {
                _person = new PersonService( new RockContext() ).Get( personId.Value );
            }

            _workflowId = PageParameter( "WorkflowId" ).AsIntegerOrNull();
            lbReturn.Visible = _workflowId.HasValue;

            dlgDetails.SaveButtonText = "Select";
            dlgDetails.SaveClick += DlgDetails_SaveClick;
            dlgDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfResourceId.ClientID );

            string script = string.Format( @"
    $('#{0}').on('click', function(e) {{
        if (typeof (Page_ClientValidate) == 'function') {{
            if ( !Page_ClientValidate('{1}') ) {{
                Page_BlockSubmit = false;
                return false;
            }}
            return true;
        }}
    }});
", hlPrint.ClientID, this.BlockValidationGroup );

            ScriptManager.RegisterStartupScript( hlPrint, hlPrint.GetType(), "printValidate", script, true );

        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMessage.Visible = false;
            nbNoResults.Visible = false;
            nbEmailMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                string searchCaptionLeft = GetAttributeValue( "SearchCaptionLeft" );
                lSearchLeftTitle.Text = searchCaptionLeft.IsNotNullOrWhitespace() ? string.Format( "<h3>{0}</h3>", searchCaptionLeft ) : string.Empty;

                string searchCaptionRight = GetAttributeValue( "SearchCaptionRight" );
                lSearchRightTitle.Text = searchCaptionRight.IsNotNullOrWhitespace() ? string.Format( "<h3>{0}</h3>", searchCaptionRight ) : string.Empty;

                GetValidPropertyValues();
                BindSelectedValues();
                ShowFilterOption();
                CreateDynamicControls( true );
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( hfResourceId.Value ) )
                {
                    dlgDetails.Show();
                }
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
            ViewState["ValidPropertyValues"] = _validPropertyValues;
            ViewState["SelectedResources"] = _selectedResources;

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
            CreateDynamicControls( true );
        }

        private void RptrSelected_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "Delete" )
            {
                _selectedResources.Remove( e.CommandArgument.ToString().AsInteger() );
                BindSelectedValues();
                BindResults( false );
            }
        }


        protected void lbReturn_Click( object sender, EventArgs e )
        {
            if ( _workflowId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var workflow = new WorkflowService( rockContext ).Get( _workflowId.Value );
                    if ( workflow != null )
                    {
                        var _returnParams = new Dictionary<string, string>() { { "WorkflowTypeId", workflow.WorkflowTypeId.ToString() }, { "WorkflowId", workflow.Id.ToString() } };
                        NavigateToLinkedPage( "WorkflowEntryPage", _returnParams );
                    }
                }
            }
        }

        protected void btnEmail_Click( object sender, EventArgs e )
        {
            // If personId was specified, save the provided resources to the workflow attribute
            if ( _selectedResources.Any() )
            {
                var selectedResourceIds = _selectedResources.Keys.ToList();

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

                var links = new List<string>();
                foreach ( var keyVal in _selectedResources )
                {
                    string url = LinkedPageUrl( "PrintPage", new Dictionary<string, string> { { "ResourceIds", keyVal.Key.ToString() } } );
                    //links.Add( string.Format( "{0} ( " + Request.Url.Scheme + "://" + Request.Url.Authority + "{1} )", keyVal.Value, url ) );
                    links.Add( string.Format( "{0}", keyVal.Value ) );
                }
                string attributeValue = links.AsDelimited( Environment.NewLine );

                using ( var rockContext = new RockContext() )
                {
                    if ( _person != null )
                    {
                        mergeFields.Add( "Person", _person );
                    }

                    if ( _workflowId.HasValue )
                    { 
                        var resourceService = new ResourceService( rockContext );
                        if ( resourceService.SaveProvidedResources( _workflowId.Value, attributeValue ) )
                        {
                            rockContext.SaveChanges();
                        }
                    }

                    var communication = GetCommunication( rockContext, _person != null ? _person.Id : (int?)null );
                    if ( communication != null )
                    {
                        string htmlBody = communication.GetMediumDataValue( "HtmlMessage" );

                        var resources = new ResourceService( rockContext ).Queryable()
                            .Where( v => selectedResourceIds.Contains( v.Id ) )
                            .OrderBy( v => v.Name )
                            .ToList();
                        if ( resources != null && resources.Any() )
                        {
                            mergeFields.Add( "Resources", resources );
                        }

                        mergeFields.Add( "IsBenevolence", ( ddlBenevolence.Visible && ( ddlBenevolence.SelectedValue.AsBooleanOrNull() ?? false ) ).ToString() );

                        communication.SetMediumDataValue( "HtmlMessage", htmlBody.ResolveMergeFields( mergeFields ) );

                        rockContext.SaveChanges();

                        NavigateToLinkedPage( "CommunicationPage", new Dictionary<string, string> { { "CommunicationId", communication.Id.ToString() } } );
                    }
                }
            }

        }

        protected void lbShowFilter_Click( object sender, EventArgs e )
        {
            pnlFilterSearch.Visible = !pnlFilterSearch.Visible;
            ShowFilterOption();
        }

        protected void lbSearch_Click( object sender, EventArgs e )
        {
            BindResults();
        }

        protected void lbClearSearch_Click( object sender, EventArgs e )
        {
            BindSelectedValues();

            CreateDynamicControls( true );

            rptrResults.Visible = false;
        }

        private void RptrResults_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var resource = e.Item.DataItem as Resource;
            if ( resource != null )
            {
                var lbSelect = e.Item.FindControl( "lbSelect" ) as LinkButton;
                var lbUnselect = e.Item.FindControl( "lbUnselect" ) as LinkButton;
                if ( lbSelect != null && lbUnselect != null )
                {
                    bool selected = _selectedResources.ContainsKey( resource.Id );
                    lbSelect.Visible = !selected;
                    lbUnselect.Visible = selected;
                }
            }
        }

        private void RptrResults_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? resourceId = e.CommandArgument.ToString().AsInteger();
            if ( resourceId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var resource = new ResourceService( rockContext ).Get( resourceId.Value );
                    if ( resource != null )
                    {
                        switch ( e.CommandName )
                        {
                            case "Select":
                                {
                                    if ( !_selectedResources.ContainsKey( resource.Id ) )
                                    {
                                        _selectedResources.Add( resource.Id, FormatHeader( resource ) );
                                    }
                                    break;
                                }

                            case "Unselect":
                                {
                                    if ( _selectedResources.ContainsKey( resource.Id ) )
                                    {
                                        _selectedResources.Remove( resource.Id );
                                    }
                                    break;
                                }

                            case "Details":
                                {
                                    dlgDetails.Title = FormatHeader( resource );

                                    hfResourceId.Value = resource.Id.ToString();

                                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                                    mergeFields.Add( "Resource", resource );

                                    string format = GetAttributeValue( "DetailsFormat" );
                                    lContent.Text = format.ResolveMergeFields( mergeFields );

                                    dlgDetails.Show();
                                }
                                break;
                        }

                        BindSelectedValues();

                        var lbSelect = e.Item.FindControl( "lbSelect" ) as LinkButton;
                        var lbUnselect = e.Item.FindControl( "lbUnselect" ) as LinkButton;
                        if ( lbSelect != null && lbUnselect != null )
                        {
                            bool selected = _selectedResources.ContainsKey( resource.Id );
                            lbSelect.Visible = !selected;
                            lbUnselect.Visible = selected;
                        }

                    }
                }
            }
        }

        private void DlgDetails_SaveClick( object sender, EventArgs e )
        {
            int? resourceId = hfResourceId.Value.AsIntegerOrNull();
            if ( resourceId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var resource = new ResourceService( rockContext ).Get( resourceId.Value );
                    if ( resource != null && !_selectedResources.ContainsKey( resource.Id ) )
                    {
                        _selectedResources.Add( resource.Id, FormatHeader( resource ) );
                        BindSelectedValues();
                        BindResults();
                    }
                }
            }

            hfResourceId.Value = string.Empty;
            dlgDetails.Hide();
        }

        protected void ddlBenevolence_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetPrintUrl();
        }

        #endregion

        #region Methods

        private void SetPrintUrl()
        {
            var pageParams = new Dictionary<string, string>();
            pageParams.Add( "ResourceIds", _selectedResources.Keys.ToList().ConvertAll<string>( k => k.ToString() ).AsDelimited( "," ) );
            if ( _workflowId.HasValue )
            {
                pageParams.Add( "WorkflowId", _workflowId.Value.ToString() );
            }
            pageParams.Add( "IsBenevolence", ( ddlBenevolence.Visible && ( ddlBenevolence.SelectedValue.AsBooleanOrNull() ?? false ) ).ToString() );
            hlPrint.NavigateUrl = LinkedPageUrl( "PrintPage", pageParams );
        }

        private void GetValidPropertyValues()
        {
            using ( var rockContext = new RockContext() )
            {
                var qry = new ResourcePropertyService( rockContext )
                    .Queryable().AsNoTracking();

                var reqDtId = GetAttributeValue( "RequiredProperty" ).AsIntegerOrNull();
                if ( reqDtId.HasValue )
                {
                    var resourceIds = new ResourceService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( r => r.ResourceProperties.Any( p => p.DefinedValue.DefinedTypeId == reqDtId ) )
                        .Select( r => r.Id );

                    qry = qry.Where( p => resourceIds.Contains( p.ResourceId ) );
                }

                _validPropertyValues = qry.Select( p => p.DefinedValueId ).Distinct().ToList();
            }
        }

        private void BindSelectedValues()
        {
            pwSelected.Visible = true;
            pwSelected.Title = _person != null ?
                string.Format( "Selected Resource(s) For <strong>{0}</strong>", _person.FullName ) :
                "Selected Resource(s)";

            if ( _selectedResources.Any() )
            {
                rptrSelected.DataSource = _selectedResources;
                rptrSelected.DataBind();

                rptrSelected.Visible = true;
                lNone.Visible = false;

                ddlBenevolence.Visible = GetAttributeValue( "IncludeBenevolenceOption" ).AsBoolean();

                SetPrintUrl();
                hlPrint.Visible = true;

                btnEmail.Visible = _person != null && _person.IsEmailActive && _person.Email.IsNotNullOrWhitespace();
            }
            else
            {
                rptrSelected.Visible = false;
                lNone.Visible = true;
                ddlBenevolence.Visible = false;
                hlPrint.Visible = false;
                btnEmail.Visible = false;
            }
        }

        private void ShowFilterOption()
        {
            lbShowFilter.Text = pnlFilterSearch.Visible
                ? "Hide Additional Criteria <i class='fa fa-chevron-up'></i>"
                : "Show Additional Criteria <i class='fa fa-chevron-down'></i>";
        }

        private void CreateDynamicControls( bool setValues )
        {
            CreateDynamicControls( phMainSearchLeft, "SearchFieldsLeft", setValues );
            CreateDynamicControls( phMainSearchRight, "SearchFieldsRight", setValues );
            CreateDynamicControls( phSecondarySearch, "AdditionalSearchFields", setValues );
            lbShowFilter.Visible = phSecondarySearch.Controls.Count > 0;
        }

        private void CreateDynamicControls( PlaceHolder ph, string attributeKey, bool setValues )
        {
            ph.Controls.Clear();

            var searchProperties = GetAttributeValue( attributeKey ).SplitDelimitedValues().ToList();
            if ( searchProperties.Any() )
            {
                foreach ( var searchProperty in searchProperties )
                {
                    int? dtId = searchProperty.AsIntegerOrNull();
                    if ( dtId.HasValue )
                    {
                        var dt = DefinedTypeCache.Read( dtId.Value );
                        if ( dt != null )
                        {
                            var dvPicker = new RockListBox { ID = string.Format( "dvFilter_{0}", dtId ) };
                            LoadDropDownValues( dvPicker, dt );
                            dvPicker.Label = dt.Name;
                            ph.Controls.Add( dvPicker );
                        }
                    }
                    else
                    {
                        switch ( searchProperty )
                        {
                            case "ResourceName":
                            case "PostalCode":
                                {
                                    var tb = new RockTextBox();
                                    tb.ID = "tb" + searchProperty;
                                    tb.Label = searchProperty.SplitCase();
                                    ph.Controls.Add( tb );
                                    break;
                                }
                            case "FirstName":
                                {
                                    var tb = new RockTextBox();
                                    tb.ID = "tb" + searchProperty;
                                    tb.Label = "Counselor's First Name";
                                    ph.Controls.Add( tb );
                                    break;
                                }
                            case "LastName":
                                {
                                    var tb = new RockTextBox();
                                    tb.ID = "tb" + searchProperty;
                                    tb.Label = "Counselor's Last Name";
                                    ph.Controls.Add( tb );
                                    break;
                                }
                            case "BenevolenceCounselor":
                            case "SupportGroupsOfferred":
                            case "SlidingFeeOffered":
                                {
                                    var ddl = new RockDropDownList();
                                    ddl.ID = "ddl" + searchProperty;
                                    ddl.Label = searchProperty.SplitCase();
                                    ddl.Items.Add( new ListItem( "", "" ) );
                                    ddl.Items.Add( new ListItem( "Yes", "True" ) );
                                    ddl.Items.Add( new ListItem( "No", "False" ) );
                                    ph.Controls.Add( ddl );
                                    break;
                                }
                            case "WillowAttender":
                                {
                                    var ddl = new RockDropDownList();
                                    ddl.ID = "ddl" + searchProperty;
                                    ddl.Label = "Counselor Attends Willow";
                                    ddl.Items.Add( new ListItem( "", "" ) );
                                    ddl.Items.Add( new ListItem( "Yes", "True" ) );
                                    ddl.Items.Add( new ListItem( "No", "False" ) );
                                    ph.Controls.Add( ddl );
                                    break;
                                }
                            case "ResourceType":
                                {
                                    var ddl = new RockDropDownList();
                                    ddl.ID = "ddl" + searchProperty;
                                    ddl.Label = searchProperty.SplitCase();
                                    ddl.BindToDefinedType( DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.REFERRAL_RESOURCE_TYPE.AsGuid() ), true );
                                    ph.Controls.Add( ddl );
                                    break;
                                }
                        }
                    }
                }
            }
        }

        private void LoadDropDownValues( RockListBox listBox, DefinedTypeCache definedType )
        {
            var selectedItems = listBox.Items.Cast<ListItem>()
                .Where( i => i.Selected )
                .Select( i => i.Value ).AsIntegerList();

            listBox.Items.Clear();

            if ( definedType != null )
            {
                foreach ( var definedValue in definedType
                    .DefinedValues
                    .Where( v => _validPropertyValues.Contains( v.Id ) )
                    .OrderBy( v => v.Order ).ThenBy( v => v.Value ) )
                {
                    var li = new ListItem( definedValue.Value, definedValue.Id.ToString() );
                    li.Selected = selectedItems.Contains( definedValue.Id );
                    listBox.Items.Add( li );
                }
            }
        }

        private List<ResourceFilterSelection> GetFilterValues()
        {
            var criteria = GetFilterValues( phMainSearchLeft, "SearchFieldsLeft" );
            criteria.AddRange( GetFilterValues( phMainSearchRight, "SearchFieldsRight" ) );
            criteria.AddRange( GetFilterValues( phSecondarySearch, "AdditionalSearchFields" ) );
            return criteria;
        }

        private List<ResourceFilterSelection> GetFilterValues( PlaceHolder ph, string attributeKey )
        {
            var results = new List<ResourceFilterSelection>();

            var searchProperties = GetAttributeValue( attributeKey ).SplitDelimitedValues().ToList();
            foreach ( var searchProperty in searchProperties )
            {
                var result = new ResourceFilterSelection();
                result.SearchProperty = searchProperty;

                int? dtId = searchProperty.AsIntegerOrNull();
                if ( dtId.HasValue )
                {
                    var dt = DefinedTypeCache.Read( dtId.Value );
                    if ( dt != null )
                    {
                        var listBox = ph.FindControl( string.Format( "dvFilter_{0}", dtId ) ) as RockListBox;
                        if ( listBox != null )
                        {
                            result.SelectedValues = new List<int>();
                            foreach ( ListItem li in listBox.Items )
                            {
                                if ( li.Selected )
                                {
                                    result.SelectedValues.Add( li.Value.AsInteger() );
                                }
                            }

                            if ( result.SelectedValues.Any() )
                            {
                                results.Add( result );
                            }
                        }
                    }
                }
                else
                {
                    switch ( searchProperty )
                    {
                        case "ResourceName":
                        case "PostalCode":
                        case "FirstName":
                        case "LastName":
                            {
                                var tb = ph.FindControl( "tb" + searchProperty ) as RockTextBox;
                                if ( tb != null )
                                {
                                    result.SelectedValue = tb.Text;
                                }
                                break;
                            }
                        case "BenevolenceCounselor":
                        case "SupportGroupsOfferred":
                        case "SlidingFeeOffered":
                        case "WillowAttender":
                        case "ResourceType":
                            {
                                var ddl = ph.FindControl( "ddl" + searchProperty ) as RockDropDownList;
                                if ( ddl != null )
                                {
                                    result.SelectedValue = ddl.SelectedValue;
                                }
                                break;
                            }

                    }

                    if ( result.SelectedValue.IsNotNullOrWhitespace() )
                    {
                        results.Add( result );
                    }

                }
            }

            return results;
        }

        private void BindResults( bool showError = true )
        {
            var criteria = GetFilterValues();

            if ( !criteria.Any() )
            {
                nbMessage.Visible = showError;
                rptrResults.Visible = false;
                return;
            }

            var rockContext = new RockContext();
            var resourceService = new ResourceService( rockContext );
            var resources = resourceService.Queryable( "ResourceProperties" ).AsNoTracking()
                .Where( r => 
                    !r.IsActive.HasValue ||
                    r.IsActive.Value == true );

            var reqDtId = GetAttributeValue( "RequiredProperty" ).AsIntegerOrNull();
            if ( reqDtId.HasValue )
            {
                resources = resources.Where( r =>
                    r.ResourceProperties.Any( p => p.DefinedValue.DefinedTypeId == reqDtId ) );
            }

            var dvIds = new List<int>();
            foreach ( var propFilter in criteria )
            {
                int? dtId = propFilter.SearchProperty.AsIntegerOrNull();
                if ( dtId.HasValue )
                {
                    dvIds.AddRange( propFilter.SelectedValues );
                }
                else
                {
                    if ( propFilter.SelectedValue.IsNotNullOrWhitespace() )
                    {
                        bool? selected = propFilter.SelectedValue.AsBooleanOrNull();

                        switch ( propFilter.SearchProperty )
                        {
                            case "ResourceName":
                                {
                                    resources = resources.Where( r => r.Name.StartsWith( propFilter.SelectedValue ) );
                                    break;
                                }
                            case "PostalCode":
                                {
                                    resources = resources.Where( r => r.PostalCode.StartsWith( propFilter.SelectedValue ) );
                                    break;
                                }
                            case "FirstName":
                                {
                                    resources = resources.Where( r => r.FirstName.StartsWith( propFilter.SelectedValue ) );
                                    break;
                                }
                            case "LastName":
                                {
                                    resources = resources.Where( r => r.LastName.StartsWith( propFilter.SelectedValue ) );
                                    break;
                                }
                            case "BenevolenceCounselor":
                                {
                                    if ( selected.HasValue )
                                    {
                                        resources = resources.Where( r => r.ReducedFeeProgramParticpant == selected.Value );
                                    }
                                    break;
                                }
                            case "SupportGroupsOfferred":
                                {
                                    if ( selected.HasValue )
                                    {
                                        resources = resources.Where( r => r.SupportGroupsOfferred == selected.Value );
                                    }
                                    break;
                                }
                            case "SlidingFeeOffered":
                                {
                                    if ( selected.HasValue )
                                    {
                                        resources = resources.Where( r => r.SlidingFeeOffered == selected.Value );
                                    }
                                    break;
                                }
                            case "WillowAttender":
                                {
                                    if ( selected.HasValue )
                                    {
                                        resources = resources.Where( r => r.WillowAttender == selected.Value );
                                    }
                                    break;
                                }
                            case "ResourceType":
                                {
                                    int? dvId = propFilter.SelectedValue.AsInteger();
                                    if ( dvId.HasValue )
                                    {
                                        resources = resources.Where( r => r.TypeValueId.HasValue && r.TypeValueId.Value == dvId.Value );
                                    }
                                    break;
                                }

                        }
                    }
                }

                if ( dvIds.Any() )
                {
                    resources = resources.Where( r => dvIds.All( d => r.ResourceProperties.Select( p => p.DefinedValueId ).Contains( d ) ) );
                }

                var resourceList = resources.ToList();
                if ( resourceList.Any() )
                {
                    rptrResults.DataSource = resources.ToList();
                    rptrResults.DataBind();
                    rptrResults.Visible = true;
                }
                else
                {
                    nbNoResults.Visible = showError;
                    rptrResults.Visible = false;
                }
            }
        }

        protected string FormatHeader( object dataItem )
        {
            var resource = dataItem as Resource;
            if ( resource != null )
            {
                if ( _responsePastorFormat && resource.LastName.IsNotNullOrWhitespace() )
                {
                    return string.Format( "{0} {1} {2}", resource.FirstName, resource.LastName,
                        resource.PropertyTypes.Where( p => p.Id == 126 ).Select( p => p.Properties ).ToList().AsDelimited( ", " ) );
                }

                return resource.Name;
            }

            return string.Empty;
        }

        protected string FormatContent( object dataItem )
        {
            var resource = dataItem as Resource;
            if ( resource != null )
            {
                if ( _responsePastorFormat  )
                {
                    return string.Format( @"
<div class='row'>
    <div class='col-md-6'>
        {0}
        {1}
        {2}
    </div>
    <div class='col-md-6'>
        <div>{3}</div>
    </div>
</div>",                
                        resource.Name.IsNotNullOrWhitespace() ? "<div>" + resource.Name + "</div>" : "",                            // {0}
                        resource.PhoneNumber.IsNotNullOrWhitespace() ? "<div>" + resource.PhoneNumber + "</div>" : "",                  // {1}
                        resource.Website.IsNotNullOrWhitespace() ?
                            "<div><a href='" + resource.Website + "' target = '_blank' >" + resource.Website + "</a></div>" : "",   // {2}
                        resource.HtmlAddress );
                }
                else
                {
                    return string.Format( @"
<div class='row margin-b-sm'>
    <div class='col-md-12'>
        {0}
    </div>
</div>
<div class='row'>
    <div class='col-md-5'>
        <div><a href = '{1}' target='_blank'>{1}</a></div>
        <div>{2}</div>
        <div><strong>{3} {4}</strong></div>
        <div>{5}</div>
    </div>
    <div class='col-md-7'>
        <div>{6}</div>
    </div>
</div>",
                        resource.Description,       // {0}
                        resource.Website,           // {1}
                        resource.PhoneNumber,       // {2}
                        resource.FirstName,         // {3}
                        resource.LastName,          // {4}
                        resource.EmailAddress,      // {5}
                        resource.HtmlAddress        // {6}
                        );
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the communication.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="peopleIds">The people ids.</param>
        /// <returns></returns>
        private Rock.Model.Communication GetCommunication( RockContext rockContext, int? personId )
        {
            var communicationService = new CommunicationService( rockContext );
            var recipientService = new CommunicationRecipientService( rockContext );
            var templateData = GetTemplateData();

            if ( templateData != null )
            {
                Rock.Model.Communication communication = new Rock.Model.Communication();
                communication.Status = CommunicationStatus.Transient;
                communication.SenderPersonAliasId = CurrentPersonAliasId;
                communicationService.Add( communication );
                communication.IsBulkCommunication = false;
                communication.MediumEntityTypeId = EntityTypeCache.Read( "Rock.Communication.Medium.Email" ).Id;
                communication.FutureSendDateTime = null;

                if ( personId.HasValue )
                {
                    // add each person as a recipient to the communication
                    var communicationRecipient = new CommunicationRecipient();
                    communicationRecipient.PersonAlias = new PersonAliasService( rockContext ).GetPrimaryAlias( personId.Value );
                    communication.Recipients.Add( communicationRecipient );
                }

                // add the MediumData to the communication
                communication.MediumData.Clear();
                foreach ( var keyVal in templateData )
                {
                    if ( !string.IsNullOrEmpty( keyVal.Value ) )
                    {
                        communication.MediumData.Add( keyVal.Key, keyVal.Value );
                    }
                }

                if ( communication.MediumData.ContainsKey( "Subject" ) )
                {
                    communication.Subject = communication.MediumData["Subject"];
                    communication.MediumData.Remove( "Subject" );
                }

                return communication;
            }

            return null;
        }

        /// <summary>
        /// Gets the template data.
        /// </summary>
        /// <exception cref="System.Exception">Missing communication template configuration.</exception>
        private Dictionary<string, string> GetTemplateData()
        {
            var template = new CommunicationTemplateService( new RockContext() ).Get( GetAttributeValue( "CommunicationTemplate" ).AsGuid() );
            if ( template == null )
            {
                nbEmailMessage.Title = "Configuration Error";
                nbEmailMessage.Text = "<p>The resource communication template is not valid.</p>";
                nbEmailMessage.Visible = true;
                nbEmailMessage.NotificationBoxType = NotificationBoxType.Danger;
                return null;
            }

            var mediumData = template.MediumData;
            if ( !mediumData.ContainsKey( "Subject" ) )
            {
                mediumData.Add( "Subject", template.Subject );
            }

            return mediumData;
        }


        #endregion

        public class ResourceFilterSelection
        {
            public string SearchProperty { get; set; }
            public string SelectedValue { get; set; }
            public List<int> SelectedValues { get; set; }
        }

    }
}