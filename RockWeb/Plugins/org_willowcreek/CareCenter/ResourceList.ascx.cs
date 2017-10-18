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
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// User controls for managing defined types and their values
    /// </summary>
    [DisplayName( "Resource List" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Lists all the resourced and allows for managing them and their values." )]

    [LinkedPage( "Detail Page", order: 0 )]
    public partial class ResourceList : RockBlock
    {
        #region Control Methods

        private List<Guid> _categoryGuids = null;
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            tFilter.ApplyFilterClick += tFilter_ApplyFilterClick;
            tFilter.DisplayFilterValue += tFilter_DisplayFilterValue;
            tFilter.Visible = true;

            gResource.DataKeyNames = new string[] { "Id" };
            gResource.Actions.ShowAdd = true;
            gResource.Actions.AddClick += gResource_Add;
            gResource.GridRebind += gResource_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gResource.Actions.ShowAdd = canAddEditDelete;
            gResource.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the tFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void tFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            tFilter.SaveUserPreference( "Organization Name", tbName.Text );
            tFilter.SaveUserPreference( "First Name", tbFirstName.Text );
            tFilter.SaveUserPreference( "Last Name", tbLastName.Text );
            tFilter.SaveUserPreference( "Owner", ddlOwner.SelectedValue );
            tFilter.SaveUserPreference( "Resource Type", ddlType.SelectedValue );
            tFilter.SaveUserPreference( "Interview Date", drpInterviewDate.DelimitedValues );
            tFilter.SaveUserPreference( "Benevolence Counselor", cbBenevolenceCounselor.Checked ? "True" : string.Empty );
            tFilter.SaveUserPreference( "Support Groups Offered", cbSupportGroupsOfferred.Checked ? "True" : string.Empty );
            tFilter.SaveUserPreference( "Sliding Fee Offerred", cbSlidingFeeOffered.Checked ? "True" : string.Empty );
            tFilter.SaveUserPreference( "Willow Attender", cbWillowAttender.Checked ? "True" : string.Empty );
            BindGrid();
        }

        void tFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Organization Name":
                case "First Name":
                case "Last Name":
                    {
                        break;
                    }
                case "Owner":
                case "Resource Type":
                    {
                        var dv = DefinedValueCache.Read( e.Value.AsInteger() );
                        e.Value = dv != null ? dv.Value : string.Empty;
                        break;
                    }
                case "Interview Date":
                    {
                        e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }
                case "Benevolence Counselor":
                case "Support Groups Offered":
                case "Sliding Fee Offerred":
                case "Willow Attender":
                    {
                        e.Value = e.Value.AsBoolean() ? "Yes" : string.Empty;
                        break;
                    }
                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        #endregion

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the Add event of the gResource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gResource_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "ResourceId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gResource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gResource_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "ResourceId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gResource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gResource_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var resourcePropertyService = new ResourcePropertyService( rockContext );
            var resourceService = new ResourceService( rockContext );

            Resource resource = resourceService.Get( e.RowKeyId );

            if ( resource != null )
            {
                string errorMessage;
                if ( !resourceService.CanDelete( resource, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                // if this Resource has ResourceProperties, see if they can be deleted
                var resourceProperties = resourcePropertyService.GetByResourceId( resource.Id ).ToList();

                foreach ( var property in resourceProperties )
                {
                    if ( !resourcePropertyService.CanDelete( property, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }
                }

                foreach ( var value in resourceProperties )
                {
                    resourcePropertyService.Delete( value );
                }

                resourceService.Delete( resource );

                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gResource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gResource_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            tbName.Text = tFilter.GetUserPreference( "Organization Name" );
            tbFirstName.Text = tFilter.GetUserPreference( "First Name" );
            tbLastName.Text = tFilter.GetUserPreference( "Last Name" );

            ddlOwner.BindToDefinedType( DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.REFERRAL_RESOURCE_OWNER.AsGuid() ), true );
            ddlOwner.SetValue( tFilter.GetUserPreference( "Owner" ) );

            ddlType.BindToDefinedType( DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.REFERRAL_RESOURCE_TYPE.AsGuid() ), true );
            ddlType.SetValue( tFilter.GetUserPreference( "Resource Type" ) );

            drpInterviewDate.DelimitedValues = tFilter.GetUserPreference( "Interview Date" );

            cbBenevolenceCounselor.Checked = tFilter.GetUserPreference( "Benevolence Counselor" ).AsBoolean();
            cbSupportGroupsOfferred.Checked = tFilter.GetUserPreference( "Support Groups Offerred" ).AsBoolean();
            cbSlidingFeeOffered.Checked = tFilter.GetUserPreference( "Sliding Fee Offered" ).AsBoolean();
            cbWillowAttender.Checked = tFilter.GetUserPreference( "Willow Attender" ).AsBoolean();
        }

        /// <summary>
        /// Binds the grid for defined types.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var queryable = new ResourceService( rockContext )
                .Queryable().AsNoTracking();

            string orgName = tFilter.GetUserPreference( "Organization Name" );
            if ( orgName.IsNotNullOrWhitespace() )
            {
                queryable = queryable.Where( r => r.Name.Contains( orgName ) );
            }

            string firstName = tFilter.GetUserPreference( "First Name" );
            if ( firstName.IsNotNullOrWhitespace() )
            {
                queryable = queryable.Where( r => r.FirstName.Contains( firstName ) );
            }

            string lastName = tFilter.GetUserPreference( "Last Name" );
            if ( lastName.IsNotNullOrWhitespace() )
            {
                queryable = queryable.Where( r => r.LastName.Contains( lastName ) );
            }

            int? ownerId = tFilter.GetUserPreference( "Owner" ).AsIntegerOrNull();
            if ( ownerId.HasValue )
            {
                queryable = queryable.Where( r => r.OwnerValueId.HasValue && r.OwnerValueId.Value == ownerId.Value );
            }

            int? typeId = tFilter.GetUserPreference( "Resource Type" ).AsIntegerOrNull();
            if ( typeId.HasValue )
            {
                queryable = queryable.Where( r => r.TypeValueId.HasValue && r.TypeValueId.Value == typeId.Value );
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( tFilter.GetUserPreference( "Interview Date" ) );
            if ( dateRange.Start.HasValue || dateRange.End.HasValue )
            {
                if ( dateRange.Start.HasValue )
                {
                    queryable = queryable
                        .Where( r => r.InterviewDateTime.HasValue && r.InterviewDateTime.Value >= dateRange.Start.Value );
                }
                if ( dateRange.End.HasValue )
                {
                    queryable = queryable
                        .Where( r => r.InterviewDateTime.HasValue && r.InterviewDateTime.Value < dateRange.End.Value );
                }
            }

            bool? benevolenceCounselor = tFilter.GetUserPreference( "Benevolence Counselor" ).AsBooleanOrNull();
            if ( benevolenceCounselor.HasValue )
            {
                queryable = queryable.Where( r => r.ReducedFeeProgramParticpant.HasValue && r.ReducedFeeProgramParticpant.Value );
            }

            bool? supportGroupsOfferred = tFilter.GetUserPreference( "Support Groups Offerred" ).AsBooleanOrNull();
            if ( supportGroupsOfferred.HasValue )
            {
                queryable = queryable.Where( r => r.SupportGroupsOfferred.HasValue && r.SupportGroupsOfferred.Value );
            }

            bool? slidingFeeOffered = tFilter.GetUserPreference( "Sliding Fee Offered" ).AsBooleanOrNull();
            if ( slidingFeeOffered.HasValue )
            {
                queryable = queryable.Where( r => r.SlidingFeeOffered.HasValue && r.SlidingFeeOffered.Value );
            }

            bool? WillowAttender = tFilter.GetUserPreference( "Willow Attender" ).AsBooleanOrNull();
            if ( WillowAttender.HasValue )
            {
                queryable = queryable.Where( r => r.WillowAttender.HasValue && r.WillowAttender.Value );
            }


            SortProperty sortProperty = gResource.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderBy( r => r.Name );
            }

            gResource.DataSource = queryable.ToList();
            gResource.DataBind();
        }

        #endregion
    }
}