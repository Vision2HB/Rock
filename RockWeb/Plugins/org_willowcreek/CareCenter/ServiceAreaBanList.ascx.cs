using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using org.willowcreek.CareCenter.Model;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Security;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// User control for managing the Service Area for the Care Center
    /// </summary>
    [DisplayName( "Service Area Ban List" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "User control for managing the Service Area Ban for the Care Center." )]
    public partial class ServiceAreaBanList : Rock.Web.UI.RockBlock
    {
        #region Private Variables

        private ServiceArea _serviceArea = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.AddConfigurationUpdateTrigger( upServiceAreaBan );

            int serviceAreaId = PageParameter( "ServiceAreaId" ).AsInteger();
            using ( var rockContext = new RockContext() )
            {
                _serviceArea = new ServiceAreaService( rockContext ).Get( serviceAreaId );
            }
            if ( _serviceArea != null )
            {
                gServiceAreaBanList.DataKeyNames = new string[] { "Id" };
                gServiceAreaBanList.Actions.AddClick += gServiceAreaBanList_Add;
                gServiceAreaBanList.GridRebind += gServiceAreasBan_GridRebind;
                bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
                gServiceAreaBanList.Actions.ShowAdd = canAddEditDelete;
                gServiceAreaBanList.IsDeleteEnabled = canAddEditDelete;

                var deleteField = new DeleteField();
                gServiceAreaBanList.Columns.Add( deleteField );
                deleteField.Click += gServiceAreasBan_Delete;
                modalServiceAreaBan.SaveClick += btnSaveServiceAreaBan_Click;
                modalServiceAreaBan.OnCancelScript = string.Format( "$('#{0}').val('');", hfServiceAreaBanId.ClientID );
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
                if ( _serviceArea != null )
                {
                    ShowDetail();
                }
                else
                {
                    pnlContent.Visible = false;
                }
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( hfServiceAreaBanId.Value ) )
                {
                    ShowServiceAreaBanValueEdit( hfServiceAreaBanId.ValueAsInt(), false );
                }
            }
        }

        #endregion

        #region Events
        /// <summary>
        /// Handles the Add event of the gServiceAreaBanList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gServiceAreaBanList_Add( object sender, EventArgs e )
        {
            gServiceAreaBanList_ShowEdit( 0 );
        }
        /// <summary>
        /// Shows the edit value.
        /// </summary>
        /// <param name="serviceAreaBanId">The value id.</param>
        protected void gServiceAreaBanList_ShowEdit( int serviceAreaBanId )
        {
            ShowServiceAreaBanValueEdit( serviceAreaBanId, true );
        }
        /// <summary>
        /// Handles the Edit event of the gServiceAreasBan control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gServiceAreasBan_Edit( object sender, RowEventArgs e )
        {
            gServiceAreaBanList_ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gServiceAreasBan control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gServiceAreasBan_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var serviceAreaBanService = new ServiceAreaBanService( rockContext );
                ServiceAreaBan serviceAreaBan = serviceAreaBanService.Get( e.RowKeyId );

                if ( serviceAreaBan != null )
                {
                    string errorMessage;
                    if ( !serviceAreaBanService.CanDelete( serviceAreaBan, out errorMessage ) )
                    {
                        mdGridWarningValues.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    serviceAreaBanService.Delete( serviceAreaBan );
                    rockContext.SaveChanges();
                }
            }

            BindServiceAreaBanGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveServiceAreaBan control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveServiceAreaBan_Click( object sender, EventArgs e )
        {
            ServiceAreaBan serviceAreaBan;
            if ( !Page.IsValid )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                ServiceAreaBanService serviceAreaBanService = new ServiceAreaBanService( rockContext );
                int serviceAreaBanId = hfServiceAreaBanId.ValueAsInt();
                if ( serviceAreaBanId.Equals( 0 ) )
                {
                    int serviceAreaId = hfServiceAreaId.ValueAsInt();
                    serviceAreaBan = new ServiceAreaBan { Id = 0 };
                    serviceAreaBan.ServiceAreaId = serviceAreaId;
                }
                else
                {
                    serviceAreaBan = serviceAreaBanService.Get( serviceAreaBanId );
                }
                if ( ppContact.PersonAliasId.HasValue )
                {
                    serviceAreaBan.PersonAliasId = ppContact.PersonAliasId.Value;
                }
                if ( dtpBanExpiryDate.SelectedDate.HasValue )
                {
                    serviceAreaBan.BanExpireDate = dtpBanExpiryDate.SelectedDate.Value;
                }

                rockContext.WrapTransaction( () =>
                 {
                     if ( serviceAreaBan.Id.Equals( 0 ) )
                     {
                         serviceAreaBanService.Add( serviceAreaBan );
                     }

                     rockContext.SaveChanges();
                 } );
            }
            BindServiceAreaBanGrid();
            hfServiceAreaBanId.Value = string.Empty;
            modalServiceAreaBan.Hide();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        public void ShowDetail()
        {
            pnlList.Visible = true;
            hfServiceAreaId.SetValue( _serviceArea.Id );
            BindServiceAreaBanGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gServiceAreasBan control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gServiceAreasBan_GridRebind( object sender, EventArgs e )
        {
            BindServiceAreaBanGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gServiceAreasBan control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void gServiceAreasBan_GridReorder( object sender, GridReorderEventArgs e )
        {
            int serviceAreaId = hfServiceAreaId.ValueAsInt();
            using ( var rockContext = new RockContext() )
            {
                var serviceAreaBanService = new ServiceAreaBanService( rockContext );
                var serviceAreasBan = serviceAreaBanService.GetByServiceAreaId( serviceAreaId );
                var changedIds = serviceAreaBanService.Reorder( serviceAreasBan.ToList(), e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindServiceAreaBanGrid();
        }



        /// <summary>
        /// Binds the Service Area Ban grid.
        /// </summary>
        protected void BindServiceAreaBanGrid()
        {
            if ( _serviceArea != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var queryable = new ServiceAreaBanService( rockContext ).GetByServiceAreaId( _serviceArea.Id );
                    SortProperty sortProperty = gServiceAreaBanList.SortProperty;
                    if ( sortProperty != null )
                    {
                        queryable = queryable.Sort( sortProperty );
                    }
                    else
                    {
                        queryable = queryable.OrderByDescending( q => q.BanExpireDate );
                    }
                    var result = queryable.ToList();
                    gServiceAreaBanList.DataSource = result;
                    gServiceAreaBanList.DataBind();
                }
            }
        }



        private void ShowServiceAreaBanValueEdit( int valueId, bool setValues )
        {
            ServiceAreaBan serviceAreaBan;
            using ( var rockContext = new RockContext() )
            {
                _serviceArea = new ServiceAreaService( rockContext ).Get( hfServiceAreaId.ValueAsInt() );
                modalServiceAreaBan.SubTitle = String.Format( "Id: {0}", valueId );
                if ( !valueId.Equals( 0 ) )
                {

                    serviceAreaBan = new ServiceAreaBanService( rockContext ).Get( valueId );
                    if ( serviceAreaBan != null )
                    {
                        lActionTitleServiceAreaBan.Text = ActionTitle.Edit( "Edit Person for " + _serviceArea.Name );
                    }

                }
                else
                {
                    serviceAreaBan = new ServiceAreaBan { Id = 0 };
                    serviceAreaBan.ServiceAreaId = hfServiceAreaId.ValueAsInt();
                    if ( _serviceArea != null )
                    {
                        lActionTitleServiceAreaBan.Text = ActionTitle.Add( "Add Person for " + _serviceArea.Name );
                    }
                }
                if ( setValues )
                {
                    hfServiceAreaBanId.SetValue( serviceAreaBan.Id );
                    if ( serviceAreaBan.PersonAlias != null )
                    {
                        ppContact.SetValue( serviceAreaBan.PersonAlias.Person );
                    }
                    else
                    {
                        ppContact.SetValue( null );
                    }
                    dtpBanExpiryDate.SelectedDate = serviceAreaBan.BanExpireDate;
                }
            }

            modalServiceAreaBan.Show();
        }

        #endregion
    }
}