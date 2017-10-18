using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;
using Rock.Security;
using org.willowcreek.CareCenter.Model;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// User controls for managing defined types
    /// </summary>
    [DisplayName( "Resource Detail" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Displays the details of the given resource." )]

    public partial class ResourceDetail : RockBlock, IDetailBlock
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
            this.AddConfigurationUpdateTrigger( upnlSettings );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Resource.FriendlyTypeName );
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
                LoadDropdowns();

                int? itemId = PageParameter( "resourceId" ).AsIntegerOrNull();
                if ( itemId.HasValue )
                {
                    ShowDetail( itemId.Value );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
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
            int? itemId = PageParameter( "resourceId" ).AsIntegerOrNull();
            if ( itemId.HasValue )
            {
                ShowDetail( itemId.Value );
            }
            else
            {
                pnlDetails.Visible = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            Resource resource = null;
            ResourceService resourceService = new ResourceService( rockContext );

            int resourceId = hfResourceId.ValueAsInt();
            if ( resourceId != 0 )
            {
                resource = resourceService.Get( resourceId );
            }

            if (resource == null )
            { 
                resource = new Resource();
                resourceService.Add( resource );
            }

            resource.Name = tbName.Text;
            resource.OwnerValueId = rblOwner.SelectedValueAsInt();
            resource.IsActive = cbIsActive.Checked;
            resource.Description = tbDescription.Text;
            resource.TypeValueId = ddlResourceType.SelectedValueAsInt();
            resource.Website = lbWebsite.Text;
            resource.Street = acAddress.Street1;
            resource.City = acAddress.City;
            resource.State = acAddress.State;
            resource.County = acAddress.County;
            resource.PostalCode = acAddress.PostalCode;
            resource.GeoPoint = geopPoint.SelectedValue;
            resource.ReducedFeeProgramParticpant = cbReducedFeeProgram.Checked;
            resource.SupportGroupsOfferred = cbSupportGroupsOfferred.Checked;
            resource.SlidingFeeOffered = cbSlidingFeeOfferred.Checked;
            resource.WillowAttender = cbWillowAttender.Checked;
            resource.FirstName = tbFirstName.Text;
            resource.LastName = tbLastName.Text;
            resource.InterviewDateTime = dtpIntervieDate.SelectedDate;
            resource.EmailAddress = ebEmail.Text;
            resource.PhoneNumber = pnbPhone.Text;
            resource.MobileNumber = pnbMobilePhone.Text;

            if ( !resource.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["ResourceId"] = resource.Id.ToString();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            ResourceService resourceService = new ResourceService( rockContext );
            Resource resource = resourceService.Get( int.Parse( hfResourceId.Value ) );

            if ( resource != null )
            {
                if ( !resource.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "Sorry, You are not authorized to delete this Resource.", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !resourceService.CanDelete( resource, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                resourceService.Delete( resource );

                rockContext.SaveChanges();

            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfResourceId.IsZero() )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                ResourceService resourceService = new ResourceService(new RockContext());
                Resource resource = resourceService.Get( hfResourceId.ValueAsInt() );
                ShowReadonlyDetails( resource );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the dropdowns.
        /// </summary>
        private void LoadDropdowns()
        {
            rblOwner.BindToDefinedType( DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.REFERRAL_RESOURCE_OWNER.AsGuid() ) );
            ddlResourceType.BindToDefinedType( DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.REFERRAL_RESOURCE_TYPE.AsGuid() ), true );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="resource">Type of the defined.</param>
        private void ShowReadonlyDetails( Resource resource )
        {
            SetEditMode( false );

            hfResourceId.SetValue( resource.Id );

            lTitle.Text = resource.Name.FormatAsHtmlTitle();
            lDescription.Text = resource.Description;
            hlInactive.Visible = !( resource.IsActive ?? true );

            var details1 = new DescriptionList();
            var details2 = new DescriptionList();

            details1.Add( "Website", resource.Website );
            details1.Add( "Address", resource.HtmlAddress );
            if ( resource.GeoPoint != null )
            {
                details1.Add( "Geopoint", string.Format( "Lat: {0}, Lon: {1}", resource.GeoPoint.Latitude, resource.GeoPoint.Longitude ) );
            }
            details1.Add( "Name", string.Format( "{0} {1}", resource.FirstName, resource.LastName ) );
            details1.Add( "Interview Date", resource.InterviewDateTime.HasValue ? resource.InterviewDateTime.Value.ToShortDateString() : "" );

            details2.Add( "Benevolence Counselor", ( resource.ReducedFeeProgramParticpant.HasValue && resource.ReducedFeeProgramParticpant.Value ) ? "Yes" : "" );
            details2.Add( "Support Groups Offerred", ( resource.SupportGroupsOfferred.HasValue && resource.SupportGroupsOfferred.Value ) ? "Yes" : "" );
            details2.Add( "Sliding Fee Offered", ( resource.SlidingFeeOffered.HasValue && resource.SlidingFeeOffered.Value ) ? "Yes" : "" );
            details2.Add( "Willow Attender", ( resource.WillowAttender.HasValue && resource.WillowAttender.Value ) ? "Yes" : "" );
            details2.Add( "Email", resource.EmailAddress );
            details2.Add( "Phone Number", resource.PhoneNumber);
            details2.Add( "Mobile Phone", resource.MobileNumber );

            lDetails1.Text = details1.Html;
            lDetails2.Text = details2.Html;
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="resource">Type of the defined.</param>
        private void ShowEditDetails( Resource resource )
        {
            if ( resource.Id > 0 )
            {
                lTitle.Text = ActionTitle.Edit( Resource.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lTitle.Text = ActionTitle.Add( Resource.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            hlInactive.Visible = !( resource.IsActive ?? true );

            SetEditMode( true );

            tbName.Text = resource.Name;
            rblOwner.SetValue( resource.OwnerValueId );
            cbIsActive.Checked = resource.IsActive ?? true;
            tbDescription.Text = resource.Description;
            ddlResourceType.SetValue( resource.TypeValueId );
            lbWebsite.Text = resource.Website;
            acAddress.Street1 = resource.Street;
            acAddress.City = resource.City;
            acAddress.State = resource.State;
            acAddress.County = resource.County;
            acAddress.PostalCode = resource.PostalCode;
            geopPoint.SelectedValue = resource.GeoPoint;
            cbReducedFeeProgram.Checked = resource.ReducedFeeProgramParticpant ?? false;
            cbSupportGroupsOfferred.Checked = resource.SupportGroupsOfferred ?? false;
            cbSlidingFeeOfferred.Checked = resource.SlidingFeeOffered ?? false;
            cbWillowAttender.Checked = resource.WillowAttender ?? false;
            tbFirstName.Text = resource.FirstName;
            tbLastName.Text = resource.LastName;
            dtpIntervieDate.SelectedDate = resource.InterviewDateTime;
            ebEmail.Text = resource.EmailAddress;
            pnbPhone.Text = resource.PhoneNumber;
            pnbMobilePhone.Text = resource.MobileNumber;
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            ResourceService resourceService = new ResourceService( new RockContext() );
            Resource resource = resourceService.Get( hfResourceId.ValueAsInt() );
            ShowEditDetails( resource );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            vsDetails.Enabled = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="resourceId">The defined type identifier.</param>
        public void ShowDetail( int resourceId )
        {
            pnlDetails.Visible = true;
            Resource resource = null;

            if ( !resourceId.Equals( 0 ) )
            {
                resource = new ResourceService( new RockContext() ).Get( resourceId );
                pdAuditDetails.SetEntity( resource, ResolveRockUrl( "~" ) );
            }

            if ( resource == null )
            {
                resource = new Resource { Id = 0 };
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfResourceId.SetValue( resource.Id );

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Resource.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( resource );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = false;
                if ( resource.Id > 0 )
                {
                    ShowReadonlyDetails( resource );
                }
                else
                {
                    ShowEditDetails( resource );
                }
            }

        }
                
        #endregion

    }
}