using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using com_northpoint.KioskTypes.Model;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Communication = Rock.Model.Communication;
using FieldType = Rock.SystemGuid.FieldType;

namespace RockWeb.Plugins.com_northpoint.RoomCheckIn
{
    [DisplayName( "Location Detail" )]
    [Category( "com_northpoint > Check-in" )]
    [Description( "Lists people checked into a location and allows check-in confirmation and editing" )]
    [IntegerField( "Refresh Interval (Seconds)", defaultValue: 10, key: "RefreshInterval" )]
    [IntegerField( "Note Type Id", "Note Type Id for Attendance records, if notes are to be shown/editable in details", required: false, key: "NoteTypeId" )]
    [IntegerField( "Minutes Passed For New Attendance", "Allow app to create multiple attendances when checking out after original schedule has passed. Blank means no new attendances will be created", false, defaultValue: 10 )]
    [DefinedTypeField( "Attendance In Room Qualifier", "Defined Type that contains values to mark attendance as 'InRoom' (Value 1: InRoom)", required: true )]
    [LinkedPage( "Location List Page", required: true )]
    [LinkedPage( "Evac Report Page", required: false )]
    [CustomEnhancedListField( "Check Out All By Schedule", "Allow a Check Out All by Schedule button for these CheckIn Configurations", @"
SELECT
    GT.Id as [Value],
    GT.Name as [Text]
FROM
    GroupType GT
INNER JOIN
    DefinedValue DV ON GT.GroupTypePurposeValueId = DV.Id
WHERE
    DV.Guid = CONVERT(UNIQUEIDENTIFIER, '" + Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE + @"')
", false )]
    [CustomEnhancedListField( "Staying Button", "Allow a Staying button on records to indicate they will stay in the room between schedules", @"
SELECT
    GT.Id as [Value],
    GT.Name as [Text]
FROM
    GroupType GT
INNER JOIN
    DefinedValue DV ON GT.GroupTypePurposeValueId = DV.Id
WHERE
    DV.Guid = CONVERT(UNIQUEIDENTIFIER, '" + Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE + @"')
", false )]
    public partial class LocationDetailAPI : RockBlock
    {
        private DateTime _startDate = RockDateTime.Today;
        private bool _usesEnRoute = true;

        private const string EN_ROUTE = "En Route";
        private const string IN_ROOM = "In Room";
        private const string HEALTH_NOTES = "Health Notes";
        private const string CHECKED_OUT = "Checked Out";

        #region ViewState

        protected bool _textingEnabled
        {
            get
            {
                if ( ViewState["_textingEnabled"] != null )
                {
                    return Convert.ToBoolean( ViewState["_textingEnabled"] );
                }
                else
                {
                    return false;
                }
            }
            set
            {
                ViewState["_textingEnabled"] = value;
            }
        }

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

        private int _locationId
        {
            get
            {
                if ( ViewState["_locationId"] != null )
                {
                    return Convert.ToInt32( ViewState["_locationId"] );
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["_locationId"] = value;
            }
        }


        protected string _roomListUrl
        {
            get
            {
                if ( ViewState["_roomListUrl"] != null )
                {
                    return Convert.ToString( ViewState["_roomListUrl"] );
                }
                else
                {
                    return "";
                }
            }
            set
            {
                ViewState["_roomListUrl"] = value;
            }
        }

        protected string _evacUrl
        {
            get
            {
                if ( ViewState["_evacUrl"] != null )
                {
                    return Convert.ToString( ViewState["_evacUrl"] );
                }
                else
                {
                    return "";
                }
            }
            set
            {
                ViewState["_evacUrl"] = value;
            }
        }

        protected string _locationName
        {
            get
            {
                if ( ViewState["_locationName"] != null )
                {
                    return Convert.ToString( ViewState["_locationName"] );
                }
                else
                {
                    return "";
                }
            }
            set
            {
                ViewState["_locationName"] = value;
            }
        }

        private List<int> _groupIds
        {
            get
            {
                if ( ViewState["_groupIds"] != null )
                {
                    return ViewState["_groupIds"].ToString().SplitDelimitedValues().AsIntegerList();
                }
                else
                {
                    return new List<int>();
                }
            }
            set
            {
                ViewState["_groupIds"] = value.AsDelimited( "," );
            }
        }

        private int _inRoomQualifierValueId
        {
            get
            {
                if ( ViewState["_inRoomQualifierValueId"] != null )
                {
                    return Convert.ToInt32( ViewState["_inRoomQualifierValueId"] );
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["_inRoomQualifierValueId"] = value;
            }
        }

        private int _stayingQualifierValueId
        {
            get
            {
                if ( ViewState["_stayingQualifierValueId"] != null )
                {
                    return Convert.ToInt32( ViewState["_stayingQualifierValueId"] );
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["_stayingQualifierValueId"] = value;
            }
        }


        #endregion

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/cordova-2.4.0.js", false );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/ZebraPrint.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

        }
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            using ( var rockContext = new RockContext() )
            {


                //Set startDate on every page refresh
                _startDate = RockDateTime.Today;

                if ( !Page.IsPostBack )
                {
                    _checkInTypeId = PageParameter( "CheckinTypeId" ).AsInteger();
                    _locationId = PageParameter( "LocationId" ).AsInteger();

                    _locationName = "Room: " + new LocationService( rockContext ).Get( _locationId ).Name;
                    lLocation.Text = _locationName;
                    lRoomCount.Text = "0 In Room";

                    if ( string.IsNullOrWhiteSpace( hfCampusId.Value ) )
                    {
                        hfCampusId.Value = new LocationService( rockContext ).Get( _locationId ).CampusId.ToString();
                    }

                    if ( string.IsNullOrWhiteSpace( hfCheckInTypeId.Value ) )
                    {
                        hfCheckInTypeId.Value = _checkInTypeId.ToString();
                    }

                    if ( string.IsNullOrWhiteSpace( hfLocationId.Value ) )
                    {
                        hfLocationId.Value = _locationId.ToString();
                    }

                    if ( string.IsNullOrWhiteSpace( hfMessages.Value ) || string.IsNullOrWhiteSpace( hfFromNumber.Value ) )
                    {
                        var checkInConfig = new GroupTypeService( new RockContext() ).Get( _checkInTypeId );
                        checkInConfig.LoadAttributes();

                        // Populate the available messages
                        if ( string.IsNullOrWhiteSpace( hfMessages.Value ) && checkInConfig.GetAttributeValue( "com_northpoint.RoomCheckIn.SMSMessageTemplates" ).IsNotNullOrWhiteSpace() )
                        {
                            //Use text value, with delimited by "|"
                            hfMessages.Value = checkInConfig.GetAttributeValue( "com_northpoint.RoomCheckIn.SMSMessageTemplates" );
                        }

                        if ( string.IsNullOrWhiteSpace( hfFromNumber.Value ) && checkInConfig.AttributeValues.ContainsKey( "com_northpoint.RoomCheckIn.SMSFrom" ) )
                        {
                            var smsFromNumber = checkInConfig.AttributeValues["com_northpoint.RoomCheckIn.SMSFrom"].Value.AsGuidOrNull();

                            if ( smsFromNumber != null )
                            {
                                hfFromNumber.Value = DefinedValueCache.Get( smsFromNumber.Value ).Guid.ToString();
                            }
                        }
                    }

                    //find qualifier value ids
                    var attendanceInRoomQualifier = DefinedTypeCache.Get( GetAttributeValue( "AttendanceInRoomQualifier" ).AsGuid() );
                    if ( attendanceInRoomQualifier.IsNotNull() )
                    {
                        var inRoom = attendanceInRoomQualifier.DefinedValues.Where( dv => dv.Value == "InRoom" ).FirstOrDefault();
                        if ( inRoom.IsNotNull() )
                        {
                            _inRoomQualifierValueId = inRoom.Id;
                        }

                        var staying = attendanceInRoomQualifier.DefinedValues.Where( dv => dv.Value == "Staying" ).FirstOrDefault();
                        if ( staying.IsNotNull() )
                        {
                            _stayingQualifierValueId = staying.Id;
                        }
                    }



                    //If Staying Button is not enabled, hide it
                    if ( GetAttributeValue( "StayingButton" ).IsNotNullOrWhiteSpace() && GetAttributeValue( "StayingButton" ).SplitDelimitedValues().AsIntegerList().Contains( _checkInTypeId ) )
                    {
                        hfStaying.Value = "True";
                    }
                    else
                    {
                        hfStaying.Value = "False";
                    }

                    //If Check Out All By Schedule is enabled
                    //show checkout ALL buttons, one for each schedule appearing
                    if ( GetAttributeValue( "CheckOutAllBySchedule" ).IsNotNullOrWhiteSpace() && GetAttributeValue( "CheckOutAllBySchedule" ).SplitDelimitedValues().AsIntegerList().Contains( _checkInTypeId ) )
                    {
                        hfCheckOutAll.Value = "True";
                    }
                    else
                    {
                        hfCheckOutAll.Value = "False";
                    }

                    //create list of groups that might use this location
                    var groupTypes = new GroupTypeService( rockContext ).GetChildGroupTypes( _checkInTypeId ).Select( t => t.Id );
                    var groups = new GroupService( rockContext ).Queryable( "GroupLocations" )
                        .Where( g => groupTypes.Contains( g.GroupTypeId ) && g.IsActive );
                    _groupIds = groups.Select( g => g.Id ).ToList();

                    // Only show SMS buttons if there is a from number and at least one predefined message
                    _textingEnabled = hfFromNumber.Value.IsNotNullOrWhiteSpace() && hfMessages.Value.IsNotNullOrWhiteSpace();

                    //Set RoomList button href from linked page in block attributes
                    if ( GetAttributeValue( "LocationListPage" ).IsNotNullOrWhiteSpace() )
                    {
                        _roomListUrl = "/page/" + new PageService( rockContext ).Get( GetAttributeValue( "LocationListPage" ).AsGuid() ).Id + "?CheckinTypeId=" + PageParameter( "CheckinTypeId" ) + "&CampusId=" + hfCampusId.Value;

                    }

                    //Set Evac Report button href from linked page in block attributes
                    if ( GetAttributeValue( "EvacReportPage" ).IsNotNullOrWhiteSpace() )
                    {
                        _evacUrl = "/page/" + new PageService( rockContext ).Get( GetAttributeValue( "EvacReportPage" ).AsGuid() ).Id + "?CheckinTypeId=" + PageParameter( "CheckinTypeId" ) + "&CampusId=" + hfCampusId.Value /*+ "&ScheduleId=" + scheduleId*/ + "&LocationId=" + _locationId;
                    }

                    //Bind grids with blank lists (Rest API will populate)
                    gEnRoute.DataSource = new List<ListItem>();
                    gEnRoute.DataBind();
                    gEnRouteOther.DataSource = new List<ListItem>();
                    gEnRouteOther.DataBind();
                    gInRoom.DataSource = new List<ListItem>();
                    gInRoom.DataBind();
                    gHealthNotes.DataSource = new List<ListItem>();
                    gHealthNotes.DataBind();
                    gCheckedOut.DataSource = new List<ListItem>();
                    gCheckedOut.DataBind();
                }
                else
                {
                    //Check postback for EventTarget
                    if ( Request["__EVENTARGUMENT"] == "ShowEditDialog")
                    {
                        ShowDialog( "EDIT" );
                        ShowEditDialog();
                    }

                }
            }

        }



        private void ShowSMSDialog( string personAliasId, string phoneNumber )
        {
            hfSMSAliasId.Value = personAliasId;
            hfPhoneNumber.Value = phoneNumber;

            var personId = hfPersonId.Value.AsIntegerOrNull();

            if ( personId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var ministry = GetMinistryName();
                    var person = new PersonService( rockContext ).Get( personId.Value );
                    var pronoun = person.Gender == Gender.Male ? "him" : person.Gender == Gender.Female ? "her" : "them";
                    var recipient = new PersonAliasService( rockContext ).GetPerson( personAliasId.AsInteger() );

                    rMessages.DataSource = hfMessages.Value.Split( '|' )
                                                     .Select( m => m.Replace( "{recipient}", recipient.NickName )
                                                                    .Replace( "{ministry}", ministry )
                                                                    .Replace( "{name}", person.NickName )
                                                                    .Replace( "{pronoun}", pronoun )
                                                            );
                    rMessages.DataBind();
                }
            }

            ShowDialog( "SMS" );
        }

        private string GetMinistryName()
        {
            var groupTypeService = new GroupTypeService( new RockContext() );
            var checkInConfig = groupTypeService.Get( _checkInTypeId );
            checkInConfig.LoadAttributes();

            var ministry = string.Empty;

            if ( checkInConfig.AttributeValues.ContainsKey( "com_northpoint.RoomCheckIn.DefaultMinistryName" ) )
            {
                ministry = checkInConfig.AttributeValues["com_northpoint.RoomCheckIn.DefaultMinistryName"].Value;
            }

            var groupTypeId = hfCheckInTypeId.Value.AsInteger();

            if ( groupTypeId != 0 )
            {
                var groupType = groupTypeService.Get( groupTypeId );
                groupType.LoadAttributes();

                if ( groupType.AttributeValues.ContainsKey( "com_northpoint.RoomCheckIn.MinistryName" ) )
                {
                    var ministryName = groupType.AttributeValues["com_northpoint.RoomCheckIn.MinistryName"].Value;

                    if ( ministryName.IsNotNullOrWhiteSpace() )
                    {
                        ministry = ministryName;
                    }
                }
            }

            return ministry;
        }

        private void ShowEditDialog( )
        {

            AllowEditsSMS( false ); //hide/show edit abilities
            pnlPIN.Visible = false; //will be enabled by button
            btnShowEdit.Visible = true; //button to show PIN

            var attendanceId = hfAttendanceId.Value.AsIntegerOrNull();

            //hfAttendanceId.Value = attendanceId.ToString();
            //hfPersonId.Value = personId.ToString();
            //hfGroupId.Value = groupId.ToString();
            //hfGroupTypeId.Value = groupTypeId.ToString();
            if ( attendanceId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var attendance = new AttendanceService( rockContext ).Get( attendanceId.Value );
                    var person = attendance.PersonAlias.Person;
                    hfPersonId.Value = person.Id.ToString();
                    person.LoadAttributes();
                    tbNickName.Text = person.NickName;
                    dpBirthdate.SelectedDate = person.BirthDate;
                    tbHealthNote.Text = person.AttributeValues["Arena-16-81"].Value;
                    tbLegalNote.Text = person.AttributeValues["LegalNotes"].Value;
                    tbAllergy.Text = person.AttributeValues["Allergy"].Value;

                    tbNotes.Text = string.Empty;


                    //Notes: Parent Location
                    tbParentLocation.Label = "Parent Location";
                    if ( person.GetAttributeValue( "com_northpoint.RoomCheckIn.LastParentLocation" ).IsNotNullOrWhiteSpace() )
                    {
                        tbParentLocation.Visible = true;
                        tbParentLocation.Text = person.GetAttributeValue( "com_northpoint.RoomCheckIn.LastParentLocation" );
                    }
                    else
                    {
                        tbParentLocation.Visible = false;
                    }

                    BindParents( rockContext, person, attendance.Id );
                }
            }
            


            // Printer list
            ddlKiosk.Items.Clear();

            //Get Printers in Devices that have a location of the selected Campus (does not iterate through parent locations to find campus, must be assigned campus directly)
            var printers = new DeviceService( new RockContext() ).GetByDeviceTypeGuid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER.AsGuid() ).ToList();

            //Filter down printers by Campus
            printers = printers.Where( p => p.Locations.Any( l => l.CampusId == hfCampusId.Value.AsIntegerOrNull() ) ).ToList();

            var listPrinters = new List<Device>();

            listPrinters.AddRange( printers );

            ddlKiosk.DataSource = listPrinters.Select( d => new { d.Id, d.Name } ).ToList();
            ddlKiosk.DataBind();
            ddlKiosk.Items.Insert( 0, new ListItem( None.Text, "-1" ) );

            var selectedPrinter = Session["LocationDetail_SelectedPrinter"];

            if ( selectedPrinter != null && ddlKiosk.Items.FindByValue( selectedPrinter.ToString() ) != null )
            {
                ddlKiosk.SelectedValue = selectedPrinter.ToString();
            }
            else if ( Request.Cookies["com_northpoint.KioskType.KioskName"] != null && Request.Cookies["com_northpoint.KioskType.KioskName"].Value.IsNotNullOrWhiteSpace() )  //Find selected kiosk printer if available
            {
                var kiosk = new KioskService( new RockContext() ).Get( Request.Cookies["com_northpoint.KioskType.KioskName"].Value.AsInteger() );
                if ( kiosk != null && listPrinters.Any( p => p.Id == kiosk.PrinterDeviceId ) )
                {
                    ddlKiosk.SelectedValue = kiosk.PrinterDeviceId.ToString();
                }
            }

            btnPrint.Enabled = ( ddlKiosk.SelectedValue.AsIntegerOrNull() >= 0 );

            ShowDialog( "EDIT" );
        }

        //protected void gEnRoute_Edit( object sender, RowEventArgs e )
        //{
        //    ShowEditDialog( e );
        //}

        //protected void gInRoom_Edit( object sender, RowEventArgs e )
        //{
        //    ShowEditDialog( e );
        //}

        //protected void gHealthNotes_Edit( object sender, RowEventArgs e )
        //{
        //    ShowEditDialog( e );
        //}

        //protected void gCheckedOut_Edit( object sender, RowEventArgs e )
        //{
        //    ShowEditDialog( e, false );
        //}

        private void BindParents( RockContext rockContext, Person person, int attendanceId )
        {
            var attendanceService = new AttendanceService( rockContext );
            var attendance = attendanceService.GetNoTracking( attendanceId );

            // Adults that checked in the child
            List<Adult> checkInAdults = new List<Adult>();
            if ( attendance.SearchResultGroup.IsNotNull() )
            {
                checkInAdults = attendance.SearchResultGroup.Members
                    .Where( x => x.Person.GetFamilyRole( rockContext ).Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()
                                 && x.Person.PhoneNumbers.Any() )
                    .Select( x => new Adult
                    {
                        ID = x.PersonId,
                        PersonAliasId = x.Person.PrimaryAliasId.Value,
                        Name = x.Person.FullName,
                        PhoneNumbers = x.Person.PhoneNumbers,
                        Sequence = 1,
                        Caption = ""
                    } )
                    .ToList();
            }


            // Adults in the child's family who are not already in the list
            var parents = person.GetFamilyMembers( false, rockContext )
                                .ToList()
                                .Where( x => x.Person.GetFamilyRole( rockContext ).Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()
                                             && x.Person.PhoneNumbers.Any()
                                             && checkInAdults.All( a => a.ID != x.PersonId )
                                      )
                                .Select( x => new Adult
                                {
                                    ID = x.PersonId,
                                    PersonAliasId = x.Person.PrimaryAliasId.Value,
                                    Name = x.Person.FullName,
                                    PhoneNumbers = x.Person.PhoneNumbers,
                                    Sequence = 2,
                                    Caption = " <small>(Parent)</small>"
                                } )
                                .ToList();

            var adults = parents.Concat( checkInAdults ).OrderBy( x => x.Sequence ).ThenBy( x => x.ID ).ToList();

            rAdults.DataSource = adults;
            rAdults.DataBind();
        }

        private void Save()
        {
            var personId = hfPersonId.Value.AsIntegerOrNull();
            if ( personId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var personService = new PersonService( rockContext );
                    var person = personService.Get( personId.Value );
                    person.LoadAttributes();

                    var attendanceId = hfAttendanceId.Value.AsIntegerOrNull();

                    person.NickName = tbNickName.Text;
                    person.SetBirthDate( dpBirthdate.SelectedDate );
                    person.AttributeValues["Allergy"].Value = tbAllergy.Text;
                    person.AttributeValues["Arena-16-81"].Value = tbHealthNote.Text;
                    person.AttributeValues["LegalNotes"].Value = tbLegalNote.Text;



                    if ( person.IsNotNull() && tbParentLocation.Text.IsNotNullOrWhiteSpace() )
                    {
                        // Update Parent Location, if changed
                        person.AttributeValues["com_northpoint.RoomCheckIn.LastParentLocation"].Value = tbParentLocation.Text;
                    }



                    person.SaveAttributeValue( "Allergy", rockContext );
                    person.SaveAttributeValue( "Arena-16-81", rockContext );
                    person.SaveAttributeValue( "LegalNotes", rockContext );
                    person.SaveAttributeValue( "com_northpoint.RoomCheckIn.LastParentLocation", rockContext );
                    rockContext.SaveChanges();
                }
            }
        }

        protected void btnSendNote_Click( object sender, EventArgs e )
        {
            //change to a workflow
            if ( tbNotes.Text.IsNotNullOrWhiteSpace() )
            {
                var rockContext = new RockContext();
                var attendanceId = hfAttendanceId.Value.AsIntegerOrNull();
                var personId = hfPersonId.Value.AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    var personService = new PersonService( rockContext );
                    var person = personService.Get( personId.Value );

                    var workflowType = new WorkflowTypeService( rockContext ).Get( "1F660BA3-9DD0-4C25-B5D5-F1829E4A78C2".AsGuid() ); // Attendance Follow-Up Workflow (Created by Migration) TODO

                    if ( workflowType.IsNotNull() )
                    {
                        Dictionary<string, string> workflowAttributes = new Dictionary<string, string>();
                        workflowAttributes.Add( "AttendanceId", attendanceId.ToStringSafe() );
                        workflowAttributes.Add( "Note", tbNotes.Text.Trim() );
                        workflowType.LaunchWorkflow( "1F660BA3-9DD0-4C25-B5D5-F1829E4A78C2".AsGuid(), "Attendance Follow-Up: " + person.FullName + _startDate, workflowAttributes );
                        rockContext.SaveChanges(); //not sure if needed or not

                        nbSendNote.Text = "Note Sent to Group Director";
                        nbSendNote.Visible = true;

                        tbNotes.Text = string.Empty;
                    }
                }

            }
        }



        protected void dlgEdit_SaveClick( object sender, EventArgs e )
        {
            Save();

            hfAttendanceId.Value = string.Empty;
            hfPersonId.Value = string.Empty;
            //hfGroupId.Value = string.Empty;
            //hfGroupTypeId.Value = string.Empty;
            nbSendNote.Visible = false;

            HideDialog();
        }


        #region Dialog

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        private void ShowDialog( string dialog )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog();
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        private void ShowDialog()
        {
            nbSendNote.Visible = false;
            switch ( hfActiveDialog.Value )
            {
                case "EDIT":
                    dlgEdit.Show();
                    break;
                case "SMS":
                    dlgSMS.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "EDIT":
                    dlgEdit.Hide();
                    break;
                case "SMS":
                    dlgSMS.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Handles the Click event of the lbLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLogin_Click( object sender, EventArgs e )
        {
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
                                AllowEditsSMS( true );
                                return;
                            }
                        }
                    }
                }
            }

            maWarning.Show( "Sorry, we couldn't find an account matching that PIN.", Rock.Web.UI.Controls.ModalAlertType.Warning );
        }

        private void AllowEditsSMS( bool allowEdits = true )
        {
            //Change fields on dialogs to allow Edits/SMS
            tbNickName.ReadOnly = !allowEdits;
            dpBirthdate.Enabled = allowEdits;
            tbAllergy.ReadOnly = !allowEdits;
            tbHealthNote.ReadOnly = !allowEdits;
            tbLegalNote.ReadOnly = !allowEdits;
            tbParentLocation.ReadOnly = !allowEdits;
            //ddlArea.Enabled = allowEdits;
            //ddlSmallGroup.Enabled = allowEdits;
            _textingEnabled = allowEdits && hfFromNumber.Value.IsNotNullOrWhiteSpace() && hfMessages.Value.IsNotNullOrWhiteSpace();

            //Show/Hide the PIN entry
            pnlPIN.Visible = !allowEdits;
            if ( allowEdits )
            {
                var rockContext = new RockContext();
                var person = new PersonService( rockContext ).Get( hfPersonId.ValueAsInt() );
                BindParents( rockContext, person, hfAttendanceId.ValueAsInt() );
            }

        }

        #endregion


        protected void btnPrint_Click( object sender, EventArgs e )
        {
            Save();

            try
            {
                var personId = hfPersonId.Value.AsIntegerOrNull();
                var attendanceId = hfAttendanceId.Value.AsIntegerOrNull();

                if ( personId.HasValue && attendanceId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var attendanceService = new AttendanceService( rockContext );
                        var attendance = attendanceService.Get( attendanceId.Value );
                        attendance.LoadAttributes();

                        var commonMergeFields = LavaHelper.GetCommonMergeFields( null );

                        var schedule = new CheckInSchedule
                        {
                            Schedule = new ScheduleService( rockContext ).Get( attendance.Occurrence.ScheduleId.Value ).Clone( false ),
                            Selected = true
                        };

                        var location = new CheckInLocation
                        {
                            Location = new LocationService( rockContext ).Get( _locationId ).Clone( false ),
                            Schedules = new List<CheckInSchedule> { schedule },
                            Selected = true
                        };

                        var group = new CheckInGroup
                        {
                            Group = new GroupService( rockContext ).Get( attendance.Occurrence.GroupId.Value ).Clone( false ),
                            Locations = new List<CheckInLocation> { location },
                            Selected = true
                        };

                        var groupType = new CheckInGroupType
                        {
                            GroupType = GroupTypeCache.Get( group.Group.GroupTypeId ),
                            Groups = new List<CheckInGroup> { group },
                            Labels = new List<CheckInLabel>(),
                            Selected = true
                        };

                        var person = new CheckInPerson
                        {
                            Person = new PersonService( rockContext ).Get( personId.Value ).Clone( false ),
                            SecurityCode = attendance.AttendanceCode.Code,
                            GroupTypes = new List<CheckInGroupType> { groupType },
                            FirstTime = attendanceService.Queryable().AsNoTracking().Count( a => a.PersonAlias.PersonId == personId.Value ) <= 1,
                            Selected = true
                        };

                        // Only print the child's tag, never the parent tag.
                        var labelCache = GetGroupTypeLabels( groupType.GroupType ).Where( l => l.LabelType == KioskLabelType.Person || l.LabelType == KioskLabelType.Location ).OrderBy( l => l.Order ).FirstOrDefault();

                        if ( labelCache != null )
                        {
                            person.SetOptions( labelCache );

                            var mergeObjects = new Dictionary<string, object>();
                            foreach ( var keyValue in commonMergeFields )
                            {
                                mergeObjects.Add( keyValue.Key, keyValue.Value );
                            }

                            mergeObjects.Add( "Location", location );
                            mergeObjects.Add( "Group", group );
                            mergeObjects.Add( "Person", person );
                            mergeObjects.Add( "GroupType", groupType );


                            foreach ( var customValue in attendance.Attributes )
                            {
                                mergeObjects.Add( customValue.Key, attendance.AttributeValues[customValue.Key].Value );
                            }

                            var groupMembers = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                                .Where( m =>
                                                    m.PersonId == person.Person.Id &&
                                                    m.GroupId == group.Group.Id )
                                                .ToList();
                            mergeObjects.Add( "GroupMembers", groupMembers );

                            //get printer
                            var printer = new DeviceService( rockContext ).Get( ddlKiosk.SelectedValue.AsInteger() );

                            var label = new CheckInLabel( labelCache, mergeObjects, person.Person.Id )
                            {
                                FileGuid = labelCache.Guid,
                                PrintTo = PrintTo.Kiosk,
                                PrinterDeviceId = printer.IsNotNull() ? ( int? ) printer.Id : null,
                                PrinterAddress = printer.IsNotNull() ? printer.IPAddress : null
                            };
                            if ( label.PrinterDeviceId.HasValue )
                            {
                                if ( label.PrintFrom == PrintFrom.Client )
                                {
                                    var urlRoot = string.Format( "{0}://{1}", Request.Url.Scheme, Request.Url.Authority );
                                    label.LabelFile = urlRoot + label.LabelFile;
                                    AddLabelScript( new List<CheckInLabel> { label }.ToJson() ); //copied from success.ascs.cs in checkin
                                }
                                else // print to IP address
                                {
                                    var printerDevice = new DeviceService( rockContext ).Get( label.PrinterDeviceId.Value );
                                    if ( printerDevice != null )
                                    {
                                        label.PrinterAddress = printerDevice.IPAddress;
                                        groupType.Labels.Add( label );

                                        ZebraPrint.PrintLabels( new List<CheckInLabel> { label } );
                                    }
                                }

                            }


                        }
                        else
                        { btnPrint.Text = "No Label Found"; }
                    }
                }
            }
            catch ( Exception ex )
            {
                LogException( ex );
            }

        }

        // Copied from CreateLabels
        private List<KioskLabel> GetGroupTypeLabels( GroupTypeCache groupType )
        {
            var labels = new List<KioskLabel>();

            foreach ( var attribute in groupType.Attributes.OrderBy( a => a.Value.Order ) )
            {
                if ( attribute.Value.FieldType.Guid == FieldType.LABEL.AsGuid() )
                {
                    var binaryFileGuid = groupType.GetAttributeValue( attribute.Key ).AsGuidOrNull();
                    if ( binaryFileGuid != null )
                    {
                        var labelCache = KioskLabel.Get( binaryFileGuid.Value );
                        labelCache.Order = attribute.Value.Order;
                        labels.Add( labelCache );
                    }
                }
            }

            return labels;
        }

        protected void ddlKiosk_SelectedIndexChanged( object sender, EventArgs e )
        {
            Session["LocationDetail_SelectedPrinter"] = ddlKiosk.SelectedValue;
            btnPrint.Enabled = ( ddlKiosk.SelectedValue.AsIntegerOrNull() >= 0 );
        }

        protected void rAdults_OnItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item )
            {
                var parentData = ( Adult ) e.Item.DataItem;
                var rptHistory = ( Repeater ) e.Item.FindControl( "rNumbers" );

                rptHistory.DataSource = parentData.PhoneNumbersOrdered.Select( x => new { PersonAliasId = x.Person.PrimaryAliasId, x.NumberFormatted, Number = SMSNumber( x ), x.NumberTypeValue } );
                rptHistory.DataBind();
            }
        }

        private string SMSNumber( PhoneNumber number )
        {
            return "+" + number.CountryCode + number.Number;
        }

        protected void rNumbers_OnItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "SMS" )
            {
                HideDialog();
                var arguments = e.CommandArgument.ToString().Split( ';' );
                ShowSMSDialog( arguments[0], arguments[1] );
            }
        }

        protected void rMessages_OnItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var rockContext = new RockContext();

            // Store the current SMS settings and change them so the chosen number is the default SMS number
            var numbers = new PersonAliasService( rockContext ).Get( hfSMSAliasId.ValueAsInt() ).Person.PhoneNumbers.ToList();

            var defaultNumber = numbers.FirstOrDefault( n => n.IsMessagingEnabled );
            var defaultNumberId = -1;

            if ( defaultNumber != null )
            {
                defaultNumberId = defaultNumber.Id;
            }

            string recipientNumber = string.Empty;
            foreach ( var number in numbers )
            {
                number.IsMessagingEnabled = string.Equals( hfPhoneNumber.Value, SMSNumber( number ) );

                if ( number.IsMessagingEnabled )
                {
                    recipientNumber = number.NumberFormatted;
                }
            }

            rockContext.SaveChanges();

            // Send the SMS
            var communication = new Communication
            {
                Recipients = new List<CommunicationRecipient>
                                                 {
                                                     new CommunicationRecipient
                                                     {
                                                         PersonAliasId = hfSMSAliasId.ValueAsInt()
                                                       , MediumEntityTypeId = 38
                                                     }
                                                 }
                                  ,
                SMSFromDefinedValueId = DefinedValueCache.Get( hfFromNumber.Value ).Id
                                  ,
                Subject = "Check-in SMS Communication"
                                  ,
                SMSMessage = e.CommandArgument.ToString()
                                  ,
                Status = CommunicationStatus.Approved
                                  ,
                CommunicationType = CommunicationType.SMS
            };
            communication.LoadAttributes();
            communication.SetAttributeValue( "AttendanceId", hfAttendanceId.Value );
            communication.SetAttributeValue( "RecipientNumber", recipientNumber );
            var communicationService = new CommunicationService( rockContext );
            communicationService.Add( communication );
            rockContext.SaveChanges();
            communication.SaveAttributeValues();

            Communication.Send( communication );

            // Change the SMS settings back to what they were
            foreach ( var number in numbers )
            {
                number.IsMessagingEnabled = number.Id == defaultNumberId;
            }

            rockContext.SaveChanges();

            HideDialog();
        }

        protected void rNumbers_OnItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item )
            {
                var button = ( LinkButton ) e.Item.FindControl( "btnSMS" );
                var span = ( HtmlGenericControl ) e.Item.FindControl( "sNumber" );

                button.Visible = _textingEnabled;
                span.Visible = !_textingEnabled;

            }
        }

        private AttendanceOccurrence GetOccurrenceForLocation( RockContext rockContext, AttendanceOccurrence occurrence, int locationId, int? groupId = null, int? scheduleId = null )
        {
            var selectedOccurrence = new AttendanceOccurrenceService( rockContext ).Get( occurrence.OccurrenceDate, groupId ?? occurrence.GroupId, locationId, scheduleId ?? occurrence.ScheduleId );

            //if not selected, create it
            if ( selectedOccurrence == null )
            {
                AttendanceOccurrence newOccurrence = new AttendanceOccurrence
                {
                    ScheduleId = scheduleId ?? occurrence.ScheduleId,
                    LocationId = locationId,
                    GroupId = groupId ?? occurrence.GroupId,
                    OccurrenceDate = occurrence.OccurrenceDate
                };
                new AttendanceOccurrenceService( rockContext ).Add( newOccurrence );
                rockContext.SaveChanges();
                selectedOccurrence = new AttendanceOccurrenceService( new RockContext() ).Get( occurrence.OccurrenceDate, groupId ?? occurrence.GroupId, locationId, scheduleId ?? occurrence.ScheduleId );
            }

            return selectedOccurrence;
        }


        /// <summary>
        /// Adds the label script for client print. Copied from Success.ascx.cs from checkin
        /// </summary>
        /// <param name="jsonObject">The json object.</param>
        private void AddLabelScript( string jsonObject )
        {
            string script = string.Format( @"

	    //if (navigator.userAgent.match(/(iPhone|iPod|iPad)/)) {{
     //       document.addEventListener('deviceready', onDeviceReady, false);
     //   }} else {{
     //       $( document ).ready(function() {{
     //           onDeviceReady();
     //       }});
     //   }}


	    // label data
        var labelData = {0};

		function onDeviceReady() {{
            try {{			
                printLabels();
            }} 
            catch (err) {{
                console.log('An error occurred printing labels: ' + err);
            }}
		}}
		
		function alertDismissed() {{
		    // do something
		}}
		
		function printLabels() {{
		    ZebraPrintPlugin.printTags(
            	JSON.stringify(labelData), 
            	function(result) {{ 
			        console.log('Tag printed');
			    }},
			    function(error) {{   
				    // error is an array where:
				    // error[0] is the error message
				    // error[1] determines if a re-print is possible (in the case where the JSON is good, but the printer was not connected)
			        console.log('An error occurred: ' + error[0]);
                    navigator.notification.alert(
                        'An error occurred while printing the labels. ' + error[0] + ' ' + JSON.stringify(labelData),  // message plus label
                        alertDismissed,         // callback
                        'Error',            // title
                        'Ok'                  // buttonName
                    );
			    }}
            );
	    }}
        
        //manually start
        onDeviceReady();

", jsonObject );
            
            ScriptManager.RegisterClientScriptBlock( upContent, upContent.GetType(), "addLabel", script, true );

        }


        protected void btnShowEdit_Click( object sender, EventArgs e )
        {
            pnlPIN.Visible = true;
            btnShowEdit.Visible = false;
        }

        #region Classes


        private class CheckInsResults
        {
            public int PersonId;
            public int? PersonAliasId;
            public string Name;
            public int AttendanceId;
            public DateTime StartDateTime;
            public DateTime? EndDateTime;
            public int? ScheduleId;
            public string Schedule;
            public string Code;
            public int? GroupId;
            public string Location;
            public int? LocationId;
            public string Group;
            public int GroupTypeId;
            public bool? InRoom;
            public bool? Staying;
            public string Allergy;
            public string SpecialNote;
        }

        public class Adult
        {
            public int ID;
            public int PersonAliasId;
            public string Name { get; set; }
            public ICollection<PhoneNumber> PhoneNumbers;
            public int Sequence;
            public string Caption { get; set; }

            public List<PhoneNumber> PhoneNumbersOrdered
            {
                get
                {
                    // Eliminate duplicate numbers that show up higher in the list
                    var numbers = new List<PhoneNumber>();
                    foreach ( var number in PhoneNumbers.OrderBy( p => p.NumberTypeValue.Value == "Mobile" ? 0 : 1 ).ThenBy( p => p.NumberTypeValue.Value == "Home" ? 0 : 1 ).ToList() )
                    {
                        if ( numbers.All( x => x.NumberFormatted != number.NumberFormatted ) )
                        {
                            numbers.Add( number );
                        }
                    }

                    return numbers;
                }
            }

        }

        #endregion

    }


}