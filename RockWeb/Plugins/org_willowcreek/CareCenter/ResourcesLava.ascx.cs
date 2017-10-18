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
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Care Center block for viewing list of appointments for a specific persons family.
    /// </summary>
    [DisplayName( "Resources Lava" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for printing resources." )]

    [CodeEditorField( "Name Template", "The Lava template to use when saving the resource name to workflow attribute.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "{{ Resource.Name }}", "", 0 )]

    [CodeEditorField( "Lava Template", "The Lava template to use for the resources.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 500, true, @"
<style>
    .food-box {
        float: right;
        width: 200px;
        height: 40px;
        margin: 5px;
        border: 1px solid rgba(0, 0, 0, .2);
    }
</style>

<div class='pull-right'>
    <a href='#' class='btn btn-lg btn-primary hidden-print margin-t-lg' onClick='window.print();'><i class='fa fa-print'></i> Print Resource(s)</a> 
</div>

<div class='row margin-b-xl'>
    <div class='col-md-6'>
        <div class='pull-left'>
            <img src='/Themes/CareCenter/Assets/Images/logo.png' width='300px' />
        </div>
    </div>
    <div class='col-md-6 text-right'>
    </div>
</div>

{% for resource in Resources %}

    <hr>
    <h3>{{ resource.Name }}</h3>
    <div class='row margin-b-sm'>
        <div class='col-md-12'>
            {{ resource.Description }}
        </div>
    </div>

    <div class='row margin-b-md'>
        <div class='col-sm-6'>
            <dl>
                {% if resource.Website and resource.Website != '' %}
                    <dt>Website</dt>
                    <dd><a href = '{{ resource.Website }}' target='_blank'>{{ resource.Website }}</a></dd>
                {% endif %}
                {% if resource.PhoneNumber and resource.PhoneNumber != '' %}
                    <dt>Phone Number</dt>
                    <dd>{{ resource.PhoneNumber }}</dd>
                {% endif %}
                {% if resource.MobileNumber and resource.MobileNumber != '' %}
                    <dt>Mobile Number</dt>
                    <dd>{{ resource.MobileNumber }}</dd>
                {% endif %}
                {% if resource.HtmlAddress and resource.HtmlAddress != '' %}
                    <dt>Address</dt>
                    <dd>{{ resource.HtmlAddress }}</dd>
                {% endif %}
            </dl>
        </div>
        <div class='col-sm-6'>
            <dl>
                {% if resource.LastName and resource.LastName != '' %}
                    <dt>Name</dt>
                    <dd>{{ resource.FirstName }} {{ resource.LastName }}</dd>
                {% endif %}
                {% if resource.EmailAddress and resource.EmailAddress != '' %}
                    <dt>Email</dt>
                    <dd>{{ resource.EmailAddress }}</dd>
                {% endif %}
            </dl>
            {% if resource.WillowAttender != null %}<div>Willow Attender: <strong>{% if resource.WillowAttender == true %}Yes{% else %}No{% endif %}</strong></div>{% endif %}
            {% if resource.SupportGroupsOfferred != null %}<div>Support Groups Offerred: <strong>{% if resource.SupportGroupsOfferred == true %}Yes{% else %}No{% endif %}</strong></div>{% endif %}
            {% if resource.ReducedFeeProgramParticpant != null %}<div>Benevolence Counselor: <strong>{% if resource.ReducedFeeProgramParticpant == true %}Yes{% else %}No{% endif %}</strong></div>{% endif %}
            {% if resource.SlidingFeeOffered != null %}<div>Sliding Fee Offerred: <strong>{% if resource.SlidingFeeOffered == true %}Yes{% else %}No{% endif %}</strong></div>{% endif %}
        </div>
    </div>

{% endfor %}
", "", 1)]
    [BooleanField( "Enable Debug", "Shows the merge fields available for the Lava", order: 2 )]
    public partial class ResourcesLava : RockBlock
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
            var resourceIds = PageParameter( "ResourceIds" ).SplitDelimitedValues().AsIntegerList();
            var workflowId = PageParameter( "WorkflowId" ).AsIntegerOrNull();

            if ( resourceIds != null && resourceIds.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

                    var resourceService = new ResourceService( rockContext );
                    var resources = new ResourceService( rockContext ).Queryable()
                        .Where( v => resourceIds.Contains( v.Id ) )
                        .OrderBy( v => v.Name )
                        .ToList();

                    if ( resources != null && resources.Any() )
                    {
                        mergeFields.Add( "Resources", resources );

                        // If workflowId was specified, save the provided resources to the workflow attribute
                        if ( workflowId.HasValue )
                        {
                            var nameTemplate = GetAttributeValue( "NameTemplate" );

                            var links = new List<string>();
                            foreach ( var resource in resources )
                            {
                                mergeFields.AddOrReplace( "Resource", resource );

                                var pageRef = new PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId );
                                pageRef.Parameters.Add( "ResourceIds", resource.Id.ToString() );
                                string url = pageRef.BuildUrl();
                                //links.Add( string.Format( "{0} ( " + Request.Url.Scheme + "://" + Request.Url.Authority + "{1} )", nameTemplate.ResolveMergeFields( mergeFields ), url ) );
                                links.Add( string.Format( "{0}", nameTemplate.ResolveMergeFields( mergeFields ) ) );
                            }
                            string attributeValue = links.AsDelimited( Environment.NewLine );

                            if ( resourceService.SaveProvidedResources( workflowId.Value, attributeValue ) )
                            {
                                rockContext.SaveChanges();
                            }
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
        }

        #endregion

    }
}