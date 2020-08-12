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
using Rock.Security;

namespace RockWeb.Plugins.com_northpoint.RoomCheckIn
{
    [DisplayName( "Launch Page" )]
    [Category( "com_northpoint > Check-in" )]
    [Description( "Starting point for Room Check-In to Launch pages from" )]
    [GroupTypesField("Selected Check-in Configs", "Select the necessary check-in group types to use Room Lists for. Will use the Default Ministry Name attribute to label", true, key:"CheckinConfigs")]
    [LinkedPage("Check-In Manager Page", key:"ManagerPage")]
    [LinkedPage( "Family Search and Registration Page", key: "RegistrationPage")]
    public partial class LaunchPage : Rock.Web.UI.RockBlock
    {
        protected int? _campusId = null;
        protected string _pageTitle = "Select Campus";

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();

            _campusId = PageParameter( "CampusId" ).AsIntegerOrNull(); // can always be overridden in url (or using campus context selector)

            if ( _campusId == null ) // Try to get context cookie
            {
                var campusEntityType = EntityTypeCache.Get( "Rock.Model.Campus" );
                var campusContext = RockPage.GetCurrentContext( campusEntityType ) as Campus;
                if ( campusContext != null )
                {
                    _campusId = campusContext.Id;

                    NavigateToCurrentPage( new Dictionary<string, string> { { "campusId", _campusId.ToString() } } );
                }
            }

            if ( _campusId == null )
            {
                //Get campus by current user family campus
                var campus = CurrentPerson.GetCampus();
                if ( campus != null )
                {
                    _campusId = campus.Id;
                }
            }
            

            //If no page Parameter or Cookie, show selectors
            if ( _campusId == null )
            {
                pnlCampus.Visible = true;
                pnlLinks.Visible = false;

                rptCampus.DataSource = CampusCache.All();
                rptCampus.DataBind();

            } else
            {
                pnlCampus.Visible = false;
                pnlLinks.Visible = true;

                CampusCache selectedCampusCache = CampusCache.Get( _campusId ?? 0 );
                _pageTitle = selectedCampusCache.Name + " - Launch Page";

                //create list for links
                List<dynamic> linkList = new List<dynamic>(); 

                //fetch Check-In Configurations
                var checkinConfigs = new GroupTypeService( rockContext ).GetByGuids( GetAttributeValues( "CheckinConfigs" ).AsGuidList() ).ToList();

                foreach ( var config in checkinConfigs )
                {
                    config.LoadAttributes();
                    linkList.Add( new
                    {
                        Name = ( config.GetAttributeValue( "com_northpoint.RoomCheckIn.DefaultMinistryName" ).IsNotNullOrWhiteSpace() ? config.GetAttributeValue( "com_northpoint.RoomCheckIn.DefaultMinistryName" ) : config.Name ) + " - Room List",
                        Id = config.Id,
                        Url = "/room-checkin/locationlist?CampusId=" + _campusId + "&CheckinTypeId=" + config.Id + "",
                        FontAwesomeIconClass = "fa fa-check-square fa-10x"
                    } );
                }

                //Add Family Registtration 
                //linkList.Add( new {
                //    Name = "Family Registration",
                //    Url = "/room-checkin/newfamily?CampusId=" + _campusId,
                //    FontAwesomeIconClass = "fa fa-address-card fa-10x"
                //} );
                


                rptLinks.DataSource = linkList;
                rptLinks.DataBind();
            }
            
        }

        private void ShowPage( bool requiresPIN, string page )
        {
            hfNextPage.Value = page;

            if ( !requiresPIN )
            {
                //Navigate to the page without pin but with current parameters
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                queryParams.Add( "campusId", _campusId.ToString() );
                NavigateToLinkedPage( page, queryParams );
            }
            else
            {
                //Show PIN dialog
                tbPIN.Text = "";
                mdlPIN.Show();
            }
        }

        
        protected void mdlPIN_SaveClick( object sender, EventArgs e )
        {
            // check if login is valid
            var pinAuth = AuthenticationContainer.GetComponent( typeof( Rock.Security.Authentication.PINAuthentication ).FullName );
            var rockContext = new Rock.Data.RockContext();
            var userLoginService = new UserLoginService( rockContext );
            var userLogin = userLoginService.GetByUserName( tbPIN.Text );
            if ( userLogin != null && userLogin.EntityTypeId.HasValue )
            {
                // make sure this is a PIN auth user login
                var userLoginEntityType = EntityTypeCache.Get( userLogin.EntityTypeId.Value );
                if ( userLoginEntityType != null && userLoginEntityType.Id == pinAuth.EntityType.Id )
                {
                    if ( pinAuth != null && pinAuth.IsActive )
                    {
                        // should always return true, but just in case
                        if ( pinAuth.Authenticate( userLogin, null ) )
                        {
                            if ( !( userLogin.IsConfirmed ?? true ) )
                            {
                                maWarning.Show( "Sorry, account needs to be confirmed.", Rock.Web.UI.Controls.ModalAlertType.Warning );
                            }
                            else if ( ( userLogin.IsLockedOut ?? false ) )
                            {
                                maWarning.Show( "Sorry, account is locked-out.", Rock.Web.UI.Controls.ModalAlertType.Warning );
                            }
                            else
                            {
                                mdlPIN.Hide();
                                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                                queryParams.Add( "campusId", _campusId.ToString() );
                                NavigateToLinkedPage( hfNextPage.Value, queryParams );
                                return;
                            }
                        }
                    }
                }
            }

            maWarning.Show( "Sorry, we couldn't find an account matching that PIN.", Rock.Web.UI.Controls.ModalAlertType.Warning );
            
        }

        protected void btnManager_Click( object sender, EventArgs e )
        {
            //hfPINnextUrl.Value = "/room-checkin/manager?campusId=" + _campusId;
            //tbPIN.Text = "";
            //mdlPIN.Show();

            ShowPage( true, "ManagerPage" );
        }

        protected void btnRegistration_Click( object sender, EventArgs e )
        {
            ShowPage( true, "RegistrationPage" );
        }
    }
}