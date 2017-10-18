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
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// User control for managing the Service Areas that are available for the Care Center
    /// </summary>
    [DisplayName( "Service Area List" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "User control for managing the Service Areas that are available for the Care Center." )]

    [LinkedPage( "Detail Page" )]
    public partial class ServiceAreaList : Rock.Web.UI.RockBlock
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
            this.AddConfigurationUpdateTrigger( upPanel );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            rGrid.DataKeyNames = new string[] { "Id" };
            rGrid.Actions.ShowAdd = true;
            rGrid.IsDeleteEnabled = UserCanEdit;
            rGrid.GridReorder += RGrid_GridReorder;
            rGrid.Actions.AddClick += rGrid_Add;
            rGrid.GridRebind += rGrid_GridRebind;

            SecurityField securityField = rGrid.Columns[6] as SecurityField;
            securityField.EntityTypeId = EntityTypeCache.Read( typeof( ServiceArea ) ).Id;
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

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            string categoryFilterValue = cpCategoriesFilter.SelectedValuesAsInt()
                .Where( v => v != 0 )
                .Select( c => c.ToString() )
                .ToList()
                .AsDelimited( "," );

            rFilter.SaveUserPreference( "Categories", categoryFilterValue );

            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Categories":

                    var categories = new List<string>();

                    foreach ( var id in e.Value.SplitDelimitedValues().AsIntegerList().Where( v => v != 0 ) )
                    {
                        var category = CategoryCache.Read( id );
                        if ( category != null )
                        {
                            categories.Add( category.Name );
                        }
                    }

                    e.Value = categories.AsDelimited( ", " );

                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the EditValue event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_RowSelected( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "ServiceAreaId", e.RowKeyValue.ToString() );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the Add event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_Add( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "ServiceAreaId", "0" );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new ServiceAreaService( rockContext );
                var serviceArea = service.Get( e.RowKeyId );
                if ( serviceArea != null )
                {
                    string errorMessage;
                    if ( !service.CanDelete( serviceArea, out errorMessage ))
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    service.Delete( serviceArea );
                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the RGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void RGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var serviceAreaService = new ServiceAreaService( rockContext );
                var list = GetData( rockContext );
                var serviceAreaIds = serviceAreaService.Reorder( list, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<ServiceArea> GetData( RockContext rockContext )
        {
            ServiceAreaService serviceAreaService = new ServiceAreaService( rockContext );
            var query = serviceAreaService.Queryable();

            var selectedCategoryIds = rFilter.GetUserPreference( "Categories" ).SplitDelimitedValues().AsIntegerList().Where( i => i != 0 ).ToList();
            if ( selectedCategoryIds.Any() )
            {
                query = query.Where( a => a.CategoryId.HasValue && selectedCategoryIds.Contains( a.CategoryId.Value ) );
            }

            return query.OrderBy( a => a.Order ).ToList();
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var serviceAreaEntityTypeId = EntityTypeCache.Read( "org.willowcreek.CareCenter.Model.ServiceArea" ).Id;
            cpCategoriesFilter.EntityTypeId = serviceAreaEntityTypeId;

            var selectedCategoryIds = rFilter.GetUserPreference( "Categories" ).SplitDelimitedValues().AsIntegerList().Where( i => i != 0 ).ToList();
            if ( selectedCategoryIds.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var selectedCategories = new CategoryService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( c => selectedCategoryIds.Contains( c.Id ) )
                        .ToList();
                    cpCategoriesFilter.SetValues( selectedCategories );
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                rGrid.DataSource = GetData( rockContext );
                rGrid.DataBind();
            }
        }

        #endregion

    }
}