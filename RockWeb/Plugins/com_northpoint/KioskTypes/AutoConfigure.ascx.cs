// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
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
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;
using com_northpoint.KioskTypes.Model;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Plugins.com_northpoint.KioskTypes
{
    [DisplayName( "AutoConfigure" )]
    [Category( "com_northpoint > Check-in" )]
    [Description( "Checkin auto configure block" )]

    [BooleanField( "Manual", "Allow for manual configuration" )]

    public partial class AutoConfigure : CheckInBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            if( !IsPostBack )
            {

                SetKiosk();
            }
        }

        private void ShowPanels()
        {
            if ( CurrentUser != null )
            {
                if ( GetAttributeValue("Manual").AsBoolean() )
                {
                    pnlMain.Visible = true;
                    pnlManual.Visible = true;
                    pnlKioskDropdown.Visible = false;
                    pnlSelect.Visible = true;
                    BindDropDownList();
                }
                else
                {
                    pnlMain.Visible = true;
                    pnlManual.Visible = false;
                    pnlKioskDropdown.Visible = true;
                    pnlSelect.Visible = true;
                    BindDropDownList();
                }
            }
        }

        private void SetKiosk( string kioskId = "" )
        {

            ShowPanels();

            using ( var rockContext = new RockContext() )
            {
                if ( Request.Cookies["com_northpoint.KioskType.KioskName"] != null && Request.Cookies["com_northpoint.KioskType.KioskName"].Value.IsNotNullOrWhiteSpace() )
                {
                    kioskId = Request.Cookies["com_northpoint.KioskType.KioskName"].Value;
                }

                var kioskService = new KioskService( rockContext );
                //If kioskId is not found, ask for a name
                if ( kioskId.IsNullOrWhiteSpace() )
                {

                }
                else
                {
                    try
                    {
                        //get kiosk from Id

                        var kiosk = kioskService.Get( kioskId.AsInteger() );
                        tbKioskName.Text = kiosk.Name;
                        ddlKioskType.SetValue( kiosk.KioskTypeId ?? 0 );
                        ddlKioskPrinter.SetValue( kiosk.PrinterDeviceId ?? 0 );
                        ddlKiosk.SetValue( kiosk.Id );

                        if ( IsPostBack ) //If page has been postback, then get Kiosk Type and redirect
                        {
                            GetKioskType( kiosk, rockContext );
                        }
                        else
                        {
                            if ( kiosk.KioskTypeId.HasValue )
                            {
                                nbWait.Visible = true;
                            }
                        }
                    }
                    catch
                    {
                        //bad or forgotten kiosk Id
                        tbKioskName.Text = "";
                        Response.Cookies["com_northpoint.KioskType.KioskName"].Value = "";
                        NavigateToCurrentPageReference();
                    }

                }
            }

            

        }

        /// <summary>
        /// Gets the Kiosk type and reroutes or set parameters
        /// </summary>
        private void GetKioskType( Kiosk kiosk, RockContext rockContext )
        {
            if ( kiosk.KioskType != null )
            {
                // If there is a checkin config, build out state
                if ( kiosk.KioskType.CheckinTemplateId.HasValue && kiosk.KioskType.GroupTypes.Select( gt => gt.Id).ToList().Any() )
                {
                    DeviceService deviceService = new DeviceService( rockContext );
                    //Load matching device and update or create information
                    var device = deviceService.Queryable().Where( d => d.Name == kiosk.Name ).FirstOrDefault();

                    //create new device to match our kiosk
                    if ( device == null )
                    {
                        device = new Device();
                        device.DeviceTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;
                        deviceService.Add( device );
                    }

                    device.Name = kiosk.Name;
                    device.IPAddress = kiosk.IPAddress;
                    device.Locations.Clear();
                    foreach ( var loc in kiosk.KioskType.Locations.ToList() )
                    {
                        device.Locations.Add( loc );
                    }
                    device.PrintFrom = kiosk.PrintFrom;
                    device.PrintToOverride = kiosk.PrintToOverride;
                    device.PrinterDeviceId = kiosk.PrinterDeviceId;
                    rockContext.SaveChanges();
                    CurrentKioskId = device.Id;
                    CurrentGroupTypeIds = kiosk.KioskType.GroupTypes.Select( gt => gt.Id ).ToList();

                    CurrentCheckinTypeId = kiosk.KioskType.CheckinTemplateId;
                    CurrentTheme = kiosk.KioskType.CheckinTheme;

                    CurrentCheckInState = null;
                    CurrentWorkflow = null;
                    Session["KioskTypeId"] = kiosk.KioskType.Id;

                    SaveState();
                }
                
                // See if Redirect Page is Set and if it matches current route.If not, redirect
                if ( kiosk.KioskType.RedirectPageId.HasValue )
                {
                    int pageId = kiosk.KioskType.RedirectPageId.Value;


                    //If the PageCache Ids do not match, navigate to the redirected page
                    if ( this.PageCache.Id != pageId )
                    {
                        PageCache redirectPage = Rock.Web.Cache.PageCache.Get( pageId );
                        //Navigate away to redirected page
                        NavigateToPage( redirectPage.Guid, new Dictionary<string, string> { { "theme", kiosk.KioskType.CheckinTheme } } );

                    }
                    else
                    {
                        NavigateToNextPage( new Dictionary<string, string> { { "theme", kiosk.KioskType.CheckinTheme } } );
                    }
                }
                else
                {

                    NavigateToNextPage( new Dictionary<string, string> { { "theme", kiosk.KioskType.CheckinTheme } } );
                }
            }
            else
            {
                ltKioskName.Text = kiosk.Name;
                
            }
        }

        protected void Timer1_Tick( object sender, EventArgs e )
        {

            SetKiosk();
        }

        protected void btnSelectKiosk_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            
            var kioskService = new KioskService( rockContext );

            //If in manual mode
            if ( GetAttributeValue( "Manual" ).AsBoolean() )
            {
                var kiosk = kioskService.GetByClientName( tbKioskName.Text );
                //if we have a matching kiosk by name, continue with 30 sec delay
                if ( kiosk.IsNotNull() )
                {
                    // If Manual option is set, get the dropdown's selection and set it up automatically
                    if ( GetAttributeValue( "Manual" ).AsBoolean() )
                    {

                        var kioskTypeService = new KioskTypeService( rockContext );
                        var kioskType = kioskTypeService.Get( ddlKioskType.SelectedValue.AsInteger() );
                        if ( kioskType != null )
                        {
                            kiosk.KioskTypeId = kioskType.Id;
                        }

                        //assign printer even if not a printer (could be blank or null)
                        kiosk.PrinterDeviceId = ddlKioskPrinter.SelectedValueAsId();

                    }
                }
                //if this is a new kiosk name, create it
                else
                {
                    kiosk = new Kiosk();
                    kiosk.Name = tbKioskName.Text;
                    kiosk.Description = "Automatically created Kiosk";
                    
                    var kioskTypeService = new KioskTypeService( rockContext );
                    var kioskType = kioskTypeService.Get( ddlKioskType.SelectedValue.AsInteger() );
                    if ( kioskType != null )
                    {
                        kiosk.KioskTypeId = kioskType.Id;
                    }

                    //assign printer even if not a printer (could be blank or null)
                    kiosk.PrintFrom = PrintFrom.Client;
                    kiosk.PrintToOverride = PrintTo.Kiosk;
                    kiosk.PrinterDeviceId = ddlKioskPrinter.SelectedValueAsId();
                    

                    kioskService.Add( kiosk );
                }

                rockContext.SaveChanges();

                //new service in case we created a new Kiosk
                rockContext = new RockContext();
                kioskService = new KioskService( rockContext );
                kiosk = kioskService.GetByClientName( tbKioskName.Text );

                Response.Cookies["com_northpoint.KioskType.KioskName"].Value = kiosk.Id.ToString();
                Response.Cookies["com_northpoint.KioskType.KioskName"].Expires = DateTime.Now.AddYears( 3 );

                //Rock.CheckIn.KioskDevice.Clear();
                
                GetKioskType( kiosk, rockContext );

            }
            else //If not in manual mode
            {
                //Get Selected Kiosk, if any

                var kiosk = kioskService.Get( ddlKiosk.SelectedValue.AsInteger() );

                if ( kiosk != null ) //if kiosk selected is valid
                {
                    Response.Cookies["com_northpoint.KioskType.KioskName"].Value = kiosk.Id.ToString();
                    Response.Cookies["com_northpoint.KioskType.KioskName"].Expires = DateTime.Now.AddYears( 3 );

                    GetKioskType( kiosk, rockContext );
                }
            }

            
            
        }

        private void BindDropDownList( Kiosk kiosk = null )
        {
            using ( var rockContext = new RockContext() )
            {
                KioskTypeService kioskTypeService = new KioskTypeService( rockContext );


                ddlKioskType.DataSource = kioskTypeService
                    .Queryable().AsNoTracking()
                    .OrderBy( t => t.Name )
                    .Select( t => new
                    {
                        t.Name,
                        t.Id
                    } )
                    .ToList();
                ddlKioskType.DataBind();

                var printers = new DeviceService( rockContext ).GetByDeviceTypeGuid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER.AsGuid() ).AsNoTracking();
                var listPrinters = printers.OrderBy( p => p.Name ).Select( p => new { Name = p.Name, Id = p.Id } ).ToList();
                ddlKioskPrinter.DataSource = listPrinters;
                ddlKioskPrinter.DataBind();
                ddlKioskPrinter.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );

                //Kiosk dropdown if not manual
                var kiosks = new KioskService( rockContext ).Queryable().AsNoTracking().Select( k => new { Name = k.Name, Id = k.Id } ).ToList();
                ddlKiosk.DataSource = kiosks;
                ddlKiosk.DataBind();
                ddlKiosk.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );
            }
            
        }
        
    }
}