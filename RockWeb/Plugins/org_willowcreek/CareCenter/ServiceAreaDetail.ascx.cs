using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;

using org.willowcreek.CareCenter.Model;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    [DisplayName( "Service Area Detail" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Displays the details of the given service area." )]

    [LinkedPage("Appointment Time Slots Page", "The page used to manage the time slots that are available for appointments", false, "", "", 0, "TimeSlotPage")]
    public partial class ServiceAreaDetail : Rock.Web.UI.RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "ServiceAreaId" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            ServiceArea serviceArea;
            using ( var rockContext = new RockContext() )
            {
                ServiceAreaService serviceAreaService = new ServiceAreaService( rockContext );
                int serviceAreaId = hfServiceAreaId.Value.AsInteger();
                if ( serviceAreaId == 0 )
                {
                    serviceArea = new ServiceArea();
                    serviceAreaService.Add( serviceArea );
                }
                else
                {
                    serviceArea = serviceAreaService.Get( serviceAreaId );
                }

                serviceArea.Name = tbName.Text;
                serviceArea.IconCssClass = tbIconCssClass.Text;
                serviceArea.UsesPassport = cbUsesPassport.Checked;
                serviceArea.WorkflowAllowsScheduling = cbWrkFlwSchedule.Checked;
                serviceArea.CategoryId = cpCategory.SelectedValueAsInt();
                serviceArea.WorkflowTypeId = wpWorkflowType.SelectedValueAsInt();
                serviceArea.IntakeLava = ceIntakeLava.Text;
                serviceArea.PassportLava = cePassportLava.Text;
                rockContext.SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var serviceArea = new ServiceAreaService( rockContext ).Get( int.Parse( hfServiceAreaId.Value ) );
            ShowEditDetails( serviceArea );
        }
        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbTimeSlots control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTimeSlots_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "TimeSlotPage", new Dictionary<string, string> { { "ServiceAreaId", hfServiceAreaId.Value } } );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="serviceAreaId">The Service Area identifier.</param>
        public void ShowDetail( int serviceAreaId )
        {
            ServiceArea serviceArea = null;
            bool editAllowed = UserCanEdit;

            if ( !serviceAreaId.Equals( 0 ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    serviceArea = new ServiceAreaService( rockContext ).Get( serviceAreaId );
                    editAllowed = editAllowed || serviceArea.IsAuthorized( Authorization.EDIT, CurrentPerson );
                    pdAuditDetails.SetEntity( serviceArea, ResolveRockUrl( "~" ) );
                }
            }

            if ( serviceArea == null )
            {
                serviceArea = new ServiceArea { Id = 0 };
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfServiceAreaId.Value = serviceArea.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Person.FriendlyTypeName );
            }

            if ( readOnly )
            {
                ShowReadonlyDetails( serviceArea );
            }
            else
            {

                if ( serviceArea.Id > 0 )
                {
                    ShowReadonlyDetails( serviceArea );
                }
                else
                {
                    ShowEditDetails( serviceArea );
                }
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="ServiceArea">The Service Area.</param>
        private void ShowReadonlyDetails( ServiceArea serviceArea )
        {
            SetEditMode( false );
            hfServiceAreaId.SetValue( serviceArea.Id );
            lActionTitle.Text = serviceArea.Name.FormatAsHtmlTitle();

            if ( !string.IsNullOrWhiteSpace( serviceArea.IconCssClass ) )
            {
                lIconHtml.Text = string.Format( "<i class='{0}' ></i>", serviceArea.IconCssClass );
            }

            DescriptionList descriptionList = new DescriptionList();
            if ( serviceArea.CategoryId.HasValue )
            {
                var category = CategoryCache.Read( serviceArea.CategoryId.Value );
                descriptionList.Add( "Category", category );
            }


            if ( serviceArea.WorkflowTypeId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var workflowTypeService = new WorkflowTypeService( rockContext );
                    var workFlowType = workflowTypeService.Get( serviceArea.WorkflowTypeId.Value );
                    descriptionList.Add( "WorkFlow Type", workFlowType );
                }
            }
            lDetailsLeft.Text = descriptionList.Html;
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="ServiceArea">The Service Area.</param>
        private void ShowEditDetails( ServiceArea serviceArea )
        {
            if ( serviceArea.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( ServiceArea.FriendlyTypeName ).FormatAsHtmlTitle();
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }
            else
            {
                lActionTitle.Text = serviceArea.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );
            LoadDropDowns();

            tbName.Text = serviceArea.Name;
            cpCategory.SetValue( serviceArea.CategoryId );
            wpWorkflowType.SetValue( serviceArea.WorkflowTypeId );
            tbIconCssClass.Text = serviceArea.IconCssClass;
            cbUsesPassport.Checked = serviceArea.UsesPassport;
            cbWrkFlwSchedule.Checked = serviceArea.WorkflowAllowsScheduling;
            ceIntakeLava.Text = serviceArea.IntakeLava;
            cePassportLava.Text = serviceArea.PassportLava;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewSummary.Visible = !editable;
            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            var serviceAreaEntityTypeId = EntityTypeCache.Read( "org.willowcreek.CareCenter.Model.ServiceArea" ).Id;
            cpCategory.EntityTypeId = serviceAreaEntityTypeId;
        }

        #endregion

    }
}