// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using org.willowcreek.CareCenter.Model;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// User controls for managing defined values
    /// </summary>
    [DisplayName( "Resource Property List" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Block for viewing the properties associated to a resource." )]
    public partial class ResourcePropertyList : RockBlock, ISecondaryBlock
    {
        #region Private Variables

        private Resource _resource = null;

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
            this.AddConfigurationUpdateTrigger( upnlSettings );

            int resourceId = PageParameter( "resourceId" ).AsInteger();
            _resource = new ResourceService( new RockContext() ).Get( resourceId );
            if ( _resource != null )
            {
                gResourceProperties.DataKeyNames = new string[] { "Id" };
                gResourceProperties.Actions.ShowAdd = false;
                gResourceProperties.IsDeleteEnabled = false;
                gResourceProperties.GridRebind += gResourceProperties_GridRebind;

                modalValue.SaveClick += btnSaveValue_Click;
                modalValue.OnCancelScript = string.Format( "$('#{0}').val('');", hfDefinedTypeId.ClientID );
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
                if ( _resource != null )
                {
                    ShowDetail();
                }
                else
                {
                    pnlList.Visible = false;
                }
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( hfDefinedTypeId.Value ) )
                {
                    modalValue.Show();
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
            int resourceId = PageParameter( "resourceId" ).AsInteger();
            _resource = new ResourceService( new RockContext() ).Get( resourceId );
            if ( _resource != null )
            {
                ShowDetail();
            }
            else
            {
                pnlList.Visible = false;
            }
        }

        /// <summary>
        /// Handles the Edit event of the gResourceProperties control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gResourceProperties_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Click event of the btnSaveResourceProperty control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveValue_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                ResourcePropertyService resourcePropertyService = new ResourcePropertyService( rockContext );

                int? definedTypeId = hfDefinedTypeId.Value.AsIntegerOrNull();
                if ( _resource != null && definedTypeId.HasValue )
                {
                    var selectedIds = dvPicker.SelectedValuesAsInt;
                    var sameIds = new List<int>();

                    foreach ( var existingProperty in resourcePropertyService
                        .GetByResourceId( _resource.Id )
                        .Where( p => p.DefinedValue.DefinedTypeId == definedTypeId )
                        .ToList() )
                    {
                        if ( selectedIds.Contains( existingProperty.DefinedValueId ) && !sameIds.Contains( existingProperty.DefinedValueId ) )
                        {
                            sameIds.Add( existingProperty.DefinedValueId );
                        }
                        else
                        {
                            resourcePropertyService.Delete( existingProperty );
                        }
                    }

                    foreach ( var newId in selectedIds.Where( i => !sameIds.Contains( i ) ) )
                    {
                        var newProperty = new ResourceProperty();
                        newProperty.ResourceId = _resource.Id;
                        newProperty.DefinedValueId = newId;
                        resourcePropertyService.Add( newProperty );
                    }

                    rockContext.SaveChanges();
                }
            }

            BindResourcePropertiesGrid();

            hfDefinedTypeId.Value = string.Empty;
            modalValue.Hide();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        public void ShowDetail()
        {
            pnlList.Visible = true;
            BindResourcePropertiesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gResourceProperties control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gResourceProperties_GridRebind( object sender, EventArgs e )
        {
            BindResourcePropertiesGrid();
        }

        /// <summary>
        /// Binds the defined values grid.
        /// </summary>
        protected void BindResourcePropertiesGrid()
        {
            if ( _resource != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    gResourceProperties.DataSource = new ResourcePropertyService( rockContext ).GetPropertySummary( _resource.Id ); 
                    gResourceProperties.DataBind();
                }
            }
        }

        private void ShowEdit( int valueId )
        {
            if ( _resource != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var categoryGuid = org.willowcreek.CareCenter.SystemGuid.DefinedType.INTAKE_OPTIONS_RESOURCE.AsGuid();
                    var propertyType = new DefinedTypeService( rockContext ).Get( valueId );
                    if ( propertyType != null )
                    {
                        var properties = new ResourcePropertyService( rockContext )
                            .GetByResourceId( _resource.Id ).AsNoTracking()
                            .Where( p => p.DefinedValue.DefinedTypeId == valueId )
                            .Select( p => p.DefinedValueId )
                            .ToList();

                        modalValue.Title = propertyType.Name;
                        hfDefinedTypeId.Value = valueId.ToString();

                        dvPicker.DefinedTypeId = valueId;
                        dvPicker.SetValues( properties );
                    }
                }
            }

            modalValue.Show();
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion

    }
}