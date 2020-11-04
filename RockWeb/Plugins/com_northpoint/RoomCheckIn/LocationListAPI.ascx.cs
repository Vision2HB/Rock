using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.Cache;

namespace RockWeb.Plugins.com_northpoint.RoomCheckIn
{
    [DisplayName( "Location List API" )]
    [Category( "com_northpoint > Check-in" )]
    [Description( "Lists all the locations with their current check-in numbers" )]
    [IntegerField( "Refresh Interval (Seconds)", defaultValue: 30, key: "RefreshInterval" )]
    [LinkedPage( "Back Page")]
    [LinkedPage( "Location Detail Page")]
    [LinkedPage( "Evac Report Page" )]
    public partial class LocationListAPI : Rock.Web.UI.RockBlock
    {

        #region ViewState

        private int _checkInTypeId
        {
            get
            {
                if ( ViewState["_checkInTypeId"] != null )
                {
                    return Convert.ToInt32( ViewState["_checkInTypeId"] );
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["_checkInTypeId"] = value;
            }
        }

        private int _campusId
        {
            get
            {
                if ( ViewState["_campusId"] != null )
                {
                    return Convert.ToInt32( ViewState["_campusId"] );
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["_campusId"] = value;
            }
        }

        private int _detailPageId
        {
            get
            {
                if ( ViewState["_detailPageId"] != null )
                {
                    return Convert.ToInt32( ViewState["_detailPageId"] );
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                ViewState["_detailPageId"] = value;
            }
        }

        
        #endregion


        protected void Page_Load( object sender, EventArgs e )
        {
            
            if ( !IsPostBack )
            {
                RockContext rockContext = new RockContext();
                //must include checkinTypeId and campusId
                _checkInTypeId = PageParameter( "CheckinTypeId" ).AsInteger();
                _campusId = PageParameter( "CampusId" ).AsInteger();

                hfCampusId.Value = _campusId.ToString();
                hfCheckInTypeId.Value = _checkInTypeId.ToString();

                
                //create Location Detail link
                if ( GetAttributeValue( "LocationDetailPage" ).IsNotNullOrWhiteSpace() )
                {
                    _detailPageId = new PageService( rockContext ).Get( GetAttributeValue( "LocationDetailPage" ).AsGuid() ).Id;
                    hfDetailsPageId.Value = _detailPageId.ToString();
                }

                //set Back button url
                if ( GetAttributeValue( "BackPage" ).IsNotNullOrWhiteSpace() )
                {
                    btnBack.PostBackUrl = "/page/" + new PageService( rockContext ).Get( GetAttributeValue( "BackPage" ).AsGuid() ).Id + "?CampusId=" + _campusId;
                }

                               
                //Bind Url to Evac button
                if ( GetAttributeValue( "EvacReportPage" ).IsNotNullOrWhiteSpace() )
                {
                    btnEvac.PostBackUrl = "/Page/" + new PageService( new RockContext() ).Get( GetAttributeValue( "EvacReportPage" ).AsGuid() ).Id + "?CheckinTypeId=" + _checkInTypeId + "&CampusId=" + _campusId ;
                }
            }

            BindGrid();
        }

        private class RoomResult
        {
            public string Area { get; set; }
            public int AreaOrder { get; set; }
            public int LocationId { get; set; }
            public string Location { get; set; }
            public int EnRoute { get; set; }
            public int InRoom { get; set; }
            public int CheckedOut { get; set; }
        }

        private void BindGrid()
        {
            List<RoomResult> roomResults = new List<RoomResult>();
            gLocations.DataSource = roomResults;
            gLocations.DataBind();

        }

    }
}