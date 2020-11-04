using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_northpoint.RoomCheckIn
{
    [DisplayName( "Parent Location Edit" )]
    [Category( "com_northpoint > Check-in" )]
    [Description( "Displays the available parent locations for each check-in configuration." )]

    public partial class ParentLocationEdit : RockBlock
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            GetData();
        }

        private void GetData()
        {
            using ( var rockContext = new RockContext() )
            {
                var groupTypes = new List<GroupType>();

                var templatePurpose = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );
                var _templatePurposeId = templatePurpose.Id;

                // Get all of the check-in template group types that user is authorized to view
                foreach ( var groupType in new GroupTypeService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( t =>
                        t.GroupTypePurposeValueId.HasValue &&
                        t.GroupTypePurposeValueId.Value == _templatePurposeId )
                    .OrderBy( t => t.Name ) )
                {
                    if ( groupType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        groupType.LoadAttributes();
                        groupTypes.Add( groupType );
                    }
                }

                var dataSource = groupTypes.Select( t => new
                {
                    t.Id,
                    t.Name,
                    ParentLocations = t.AttributeValues["com_northpoint.RoomCheckIn.ParentLocations"]
                    //IconCssClass = string.IsNullOrWhiteSpace( t.IconCssClass ) ? "fa fa-sign-in" : t.IconCssClass,
                    //ActiveCssClass = t.Id == activeTypeId ? "active" : ""
                } ).ToList();

                gParentLocations.DataSource = dataSource;

                gParentLocations.DataBind();
            }
        }

        protected void gParentLocations_Edit( object sender, RowEventArgs e )
        {
            var id = e.RowKeyValues[0] as int?;
            var locations = e.RowKeyValues[1].ToString();

            hfConfigId.Value = id.ToString();

            tbLocations.Text = locations;

            dlgEdit.Show();
        }

        protected void dlgEdit_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupTypeService = new GroupTypeService( rockContext );
                var config = groupTypeService.Get( hfConfigId.Value.AsInteger() );
                config.LoadAttributes();
                config.SetAttributeValue( "com_northpoint.RoomCheckIn.ParentLocations", tbLocations.Text );
                config.SaveAttributeValue( "com_northpoint.RoomCheckIn.ParentLocations", rockContext );
            }

            GetData();
            hfConfigId.Value = string.Empty;
            dlgEdit.Hide();
        }
    }
}